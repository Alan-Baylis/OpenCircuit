using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

namespace Vox {

	[ExecuteInEditMode]
	[System.Serializable]
	public class RendererDict : SerializableDictionary<Index, VoxelRenderer> { }

	[AddComponentMenu("")]
	[ExecuteInEditMode]
	public class Tree : MonoBehaviour, ISerializationCallbackReceiver {

		public const ulong FILE_FORMAT_VERSION = 2;

		[System.NonSerialized]
		public readonly static HashSet<Tree> generatingTrees = new HashSet<Tree>();

		// basic stats
		public byte isoLevel = 127;
		public float lodDetail = 1;
		public bool useLod = false;
		public GameObject trees;
		public float treeDensity = 0.02f;
		public float treeSlopeTolerance = 5;
		[HideInInspector]
		public Camera cam;
		public float curLodDetail = 10f;
		public VoxelSubstance[] voxelSubstances;
		public VoxelMask[] masks;
		public bool createColliders = true;
		public bool useStaticMeshes = true;
		public bool saveMeshes = false;
		public bool reduceMeshes = false;
		public float reductionAmount = 0.1f;

		public byte maximumDetail {
			get { return maxDetail; }
			set { maxDetail = value; setupLookupTables(); }
		}
		public float width {
			get { return baseSize; }
			set { baseSize = value; setupLookupTables(); }
		}


		// performance stats
//		private int treeCount = 0;

		// voxel data
		[HideInInspector]
		public VoxelBlock head;
		[HideInInspector]
		public float[] sizes;
		[HideInInspector]
		public uint dimmension;
		[System.NonSerialized]
		public RendererDict renderers = new RendererDict();
		public byte[] voxelData = new byte[0];
		[System.NonSerialized]
		public bool dirty = true;
		[System.NonSerialized]
		public int vertexCount = 0;
		[System.NonSerialized]
		public int triangleCount = 0;

		[System.NonSerialized]
		public double meshGenTime = 0;
		[System.NonSerialized]
		public double meshGenArrayTime = 0;
		[System.NonSerialized]
		public double meshApplyTime = 0;

		[System.NonSerialized]
		public int meshGenCount = 0;
		[System.NonSerialized]
		public int meshApplyCount = 0;

		[SerializeField]
		private byte maxDetail = 7;
		[SerializeField]
		private float baseSize = 128;

		[System.NonSerialized]
		private Queue<VoxelJob> jobQueue = new Queue<VoxelJob>(100);
		private Vector3 localCamPosition;
		[System.NonSerialized]
		private int updateCheckJobs;
		[System.NonSerialized]
		private bool generationPaused = false;
		[System.NonSerialized]
		private bool rebakedLighting = false;

		public void Start() {
			setupLookupTables();
		}



		// test values
		private int updateCounter = 0;

		public virtual void initialize() {

			//			float startTime = Time.realtimeSinceStartup;

			// setup lookup tables, etc.
			setupLookupTables();
			updateCheckJobs = 0;

			// initialize voxels
			head = new VoxelBlock();
//			genData(0);

//			float endTime = Time.realtimeSinceStartup;


//			print("Voxel Gen time:                   " + (endTime - startTime));
//			print("Average Voxel Opacity:            " + head.averageOpacity());
//			print("Total Voxel Blocks:               " + VoxelHolder.blockCount);
//			print("Total Tree Count:                 " + treeCount);
//			print("Renderer Count:                   " + VoxelRenderer.rendCount);
//			print("Duplicate Triangle Count:         " + VoxelRenderer.duplicateTriangleCount);
		}

		public void setupLookupTables() {
			sizes = new float[maximumDetail + 1];
			float s = baseSize;
			for (int i = 0; i <= maximumDetail; ++i) {
				sizes[i] = s;
				s /= 2;
			}
			dimmension = (uint)(1 << maximumDetail);
        }

		public void Update() {
//			if (Application.isPlaying) {
//				if (updateCounter == 0) {
//					if (useLod) {
//						if (curLodDetail < lodDetail) {
//							if (lodDetail - curLodDetail < 0.1f) {
//								if (lodDetail - curLodDetail > -0.1f)
//									curLodDetail = lodDetail;
//								else
//									curLodDetail -= 0.1f;
//							} else
//								curLodDetail += 0.1f;
//						}
////						updateLocalCamPosition();
//					}
//					if (updateCheckJobs < 1)
//						enqueueCheck(new UpdateCheckJob(head, this, 0));
//				}
//				updateCounter = (updateCounter + 1) % 2;
//			}
			applyQueuedMeshes();
			if (jobQueue.Count < 1)
				generatingTrees.Remove(this);
			if (generationPaused) {
				if (VoxelThread.getJobCount() < 1 && jobQueue.Count < 1) {
					//if (!rebakedLighting) {
					//	UnityEditor.Lightmapping.Bake();
					//	rebakedLighting = true;
					//} else if (!UnityEditor.Lightmapping.isRunning) {
						generationPaused = false;
						Time.timeScale = 1;
					//}
				}
			}
		}

		public void enqueueCheck(VoxelJob job) {
			VoxelThread.enqueueUpdate(job);
		}

		public void enqueueUpdate(VoxelJob job) {
			VoxelThread.enqueueUpdate(job);
		}

		public void applyQueuedMeshes() {
			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			watch.Start();
			while (jobQueue.Count > 0 && watch.ElapsedMilliseconds < 20) {
				lock(this) {
					jobQueue.Dequeue().execute();
				}
			}
			watch.Stop();
		}

		public float getLodDetail() {
			return curLodDetail;
		}

		public VoxelBlock getHead() {
			return head;
		}

		public VoxelRenderer getRenderer(Index index) {
			VoxelRenderer rend = null;
			return renderers.TryGetValue(index, out rend)?
				rend : null;
		}

//		public void updateLocalCamPosition() {
//			lock(this) {
//				cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
//			}
//			localCamPosition = transform.TransformPoint(cam.transform.position);
//		}

		public Vector3 getLocalCamPosition() {
//			lock(this) {
//				return localCamPosition;
//			}
			return Vector3.zero;
		}

		public VoxelUpdateInfo getBaseUpdateInfo() {
			return new VoxelUpdateInfo(sizes[0], head, this);
		}

		public Vector3 globalToVoxelPosition(Vector3 globalPosition) {
			return transform.InverseTransformPoint(globalPosition) / voxelSize();
		}

		public Vector3 voxelToGlobalPosition(Vector3 voxelPosition) {
			return transform.TransformPoint(voxelPosition * voxelSize());
		}

		public float voxelSize() {
			return sizes[maximumDetail];
		}

		public Voxel[,,] getArray(uint xMin, uint yMin, uint zMin, uint xMax, uint yMax, uint zMax) {
			if (xMax > dimmension)
				xMax = dimmension;
			if (yMax > dimmension)
				yMax = dimmension;
			if (zMax > dimmension)
				zMax = dimmension;
			Voxel[,,] array = new Voxel[xMax -xMin, yMax -yMin, zMax -zMin];
			head.putInArray(maximumDetail, ref array, 0, 0, 0, xMin, yMin, zMin, xMax, yMax, zMax);
			return array;
		}

		internal void addUpdateCheckJob() {
			++updateCheckJobs;
		}

		internal void removeUpdateCheckJob() {
			--updateCheckJobs;
		}

		public void clearRenderers() {
			lock(this) {
				while(renderers.Count > 0) {
					Dictionary<Index, VoxelRenderer>.ValueCollection.Enumerator e = renderers.Values.GetEnumerator();
					e.MoveNext();
					e.Current.clear();
				}
			}
			List<Transform> children = new List<Transform>(transform.childCount);
			foreach(Transform child in transform) {
				if ((child.hideFlags & HideFlags.HideInHierarchy) != 0)
					children.Add(child);
			}
			foreach(Transform child in children) {
				GameObject.DestroyImmediate(child.gameObject);
			}
//			foreach(MeshCollider collider in GetComponents<MeshCollider>()) {
//				if ((collider.hideFlags & HideFlags.HideInInspector) != 0)
//					GameObject.DestroyImmediate(collider);
//			}
			vertexCount = 0;
			triangleCount = 0;
		}

		public void generateRenderers() {
			clearRenderers();
			//updateLocalCamPosition();
			enqueueCheck(new UpdateCheckJob(head, this, 0));
		}

		public void wipe() {
			clearRenderers();
			if (head != null) {
				head = null;
			}
			dirty = true;
		}

		//public void OnGUI() {
		//	GUI.Label(new Rect(0, 200, 200, 20), "Voxel Triangle Count: " + VoxelRenderer.triangleCount);
		//	GUI.Label(new Rect(0, 220, 200, 20), "Voxel Vertex Count: " + VoxelRenderer.vertexCount);
		//	GUI.Label(new Rect(0, 240, 200, 20), "Voxel Duplicate Triangle Count: " + VoxelRenderer.duplicateTriangleCount);
		//}

		public void OnBeforeSerialize() {
			lock(this) {
				//print("Serializing Voxels");
				if (voxelData.Length < 1 || dirty || head == null) {
					//print("Voxels dirty");
					dirty = false;
					voxelData = new byte[0];
					if (getHead() != null) {
						MemoryStream stream = new MemoryStream();
						BinaryWriter writer = new BinaryWriter(stream);
						getHead().serialize(writer);
						voxelData = stream.ToArray();
						stream.Close();
					}
				}
			}
		}

		public void OnAfterDeserialize() {
			//			clearRenderers();
			//print("Deserializing");
			lock(this) {
				if (voxelData.Length > 0) {
					MemoryStream stream = new MemoryStream(voxelData);
					BinaryReader reader = new BinaryReader(stream);
					head = (VoxelBlock)VoxelHolder.deserialize(reader);
					stream.Close();
				}

				// relink renderers
				//relinkRenderers();
				enqueueJob(new LinkRenderersJob(this));
			}
		}

		public Dictionary<Index, List<GameObject>> findRendererObjects() {
			Dictionary<Index, List<GameObject>> meshes = new Dictionary<Index, List<GameObject>>();
			foreach (Transform child in transform) {
				VoxelMeshObject meshObject = child.GetComponent<VoxelMeshObject>();
				if (meshObject == null)
					continue;
				List<GameObject> objects;
				//if (meshObject.index == null) {
				//	print("wow");
				//	continue;
				//}
				//print("Found valid child");
				meshes.TryGetValue(meshObject.index, out objects);
				if (objects == null) {
					objects = new List<GameObject>();
					meshes[meshObject.index] = objects;
				}
				objects.Add(meshObject.gameObject);
			}
			return meshes;
		}

		public void relinkRenderers() {
			relinkRenderers(findRendererObjects());
		}

		public void relinkRenderers(Dictionary<Index, List<GameObject>> meshes) {
			lock(this) {
				//print("Start Renderers: " + renderers.Count);
				foreach (Index index in meshes.Keys) {
					List<GameObject> objects = meshes[index];
					//print("Mesh object count: " + objects.Count);
					VoxelRenderer rend;
					renderers.TryGetValue(index, out rend);
					if (rend == null) {
						rend = new VoxelRenderer(index, this);
						renderers[index] = rend;
					//} else {
					//	print("already had renderer");
					}
					rend.obs = objects.ToArray();
				}
				//print("End Renderers: " + renderers.Count);
			}
		}

		public bool import(string fileName) {
			clearRenderers();
			Stream stream = File.OpenRead(fileName);
			BinaryReader reader = new BinaryReader(stream);
			ulong fileFormatVersion = reader.ReadUInt64();
			if (fileFormatVersion != FILE_FORMAT_VERSION
				&& fileFormatVersion != 1) {
				stream.Close ();
				print("Wrong voxel file format version: " +fileFormatVersion +", should be " +FILE_FORMAT_VERSION);
				return false;
			} else {
				// read meta data
				if (fileFormatVersion > 1) {
					maximumDetail = reader.ReadByte();
					int substanceCount = reader.ReadInt32();
					if (substanceCount > voxelSubstances.Length)
						System.Array.Resize(ref voxelSubstances, substanceCount);
					setupLookupTables();
				}

				// read voxel data
				head = (VoxelBlock)VoxelHolder.deserialize(reader);
				dirty = true;
				stream.Close();
				return true;
			}
		}

		public void export(string fileName) {
			if (getHead() != null) {
				Stream stream = File.Create(fileName);
				BinaryWriter writer = new BinaryWriter(stream);

				// write meta data
				writer.Write(FILE_FORMAT_VERSION);
				writer.Write(maxDetail);
				writer.Write(voxelSubstances.Length);

				// write voxel data
				getHead().serialize(writer);
				stream.Close();
			}
		}

		public bool generating() {
			return VoxelThread.getJobCount() > 0 || jobQueue.Count > 0;
		}

		public int getJobCount() {
			return jobQueue.Count;
		}

		public void pauseForGeneration() {
			if (!Application.isPlaying)
				return;
			generationPaused = true;
			rebakedLighting = false;
			Time.timeScale = 0;
		}

		internal  void enqueueJob(VoxelJob job) {
			lock(this) {
				generatingTrees.Add(this);
				jobQueue.Enqueue(job);
			}
		}
	}

}
