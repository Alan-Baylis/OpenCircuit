using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace Vox {

	[ExecuteInEditMode]
	[System.Serializable]
	public class RendererDict : SerializableDictionary<Index, VoxelRenderer> { }

	[AddComponentMenu("")]
	[ExecuteInEditMode]
	public class OcTree : MonoBehaviour, ISerializationCallbackReceiver {

		public const ulong FILE_FORMAT_VERSION = 2;

		[System.NonSerialized]
		public static readonly HashSet<OcTree> generatingTrees = new HashSet<OcTree>();



		// configuration
		public byte isoLevel = 127;
		public float lodDetail = 1;
		public bool useLod;
		public GameObject trees;
		public float treeDensity = 0.02f;
		public float treeSlopeTolerance = 5;
		public float curLodDetail = 10f;
		public VoxelSubstance[] voxelSubstances;
		public VoxelMask[] masks;
		public bool createColliders = true;
		public bool useStaticMeshes = true;
		public bool saveMeshes;
		public bool reduceMeshes;
		public float reductionAmount = 0.1f;
		public byte maxDepth = 7;
		public byte renderDepth = 7;
		public float width = 128;



		// voxel data
		private VoxelBlock realHead;
		[System.NonSerialized]
		public RendererDict renderers = new RendererDict();
		[System.NonSerialized]
		public bool dirty = true;
		[SerializeField]
		private byte[] voxelData = new byte[0];
		[System.NonSerialized]
		private Queue<VoxelJob> jobQueue = new Queue<VoxelJob>(100);
		[System.NonSerialized]
		private bool generationPaused = false;



		// polygonization stats
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
		[System.NonSerialized]
		private int updateCheckJobs = 0;



		// read operations

		public float getVoxelSize(int depth) {
			return width / (1 << depth);
		}

		public uint dimmension { get {
			return (uint)(1 << maxDepth);
        } }

		public float voxelSize { get {
			return getVoxelSize(maxDepth);
		} }

		public float voxelRenderSize { get {
			return getVoxelSize(renderDepth);
		} }

		public VoxelBlock head { get {
			return realHead;
		} }

		public bool hasVoxelData { get {
			return realHead != null;
		} }

		public bool hasRenderers { get {
			return renderers.Count > 0;
		} }

		public VoxelRenderer getRenderer(Index index) {
			VoxelRenderer rend = null;
			return renderers.TryGetValue(index, out rend)?
				rend : null;
		}

		public uint getDimmension(byte depth) {
			return (uint)(1 << depth);
		}

		public Vector3 globalToVoxelPosition(Vector3 globalPosition) {
			return transform.InverseTransformPoint(globalPosition) / voxelSize;
		}

		public Vector3 voxelToGlobalPosition(Vector3 voxelPosition) {
			return transform.TransformPoint(voxelPosition * voxelSize);
		}

		public bool generating() {
			return VoxelThread.getJobCount() > 0 || jobQueue.Count > 0;
		}

		public int getJobCount() {
			return jobQueue.Count;
		}

		public Dictionary<Index, List<GameObject>> findRendererObjects() {
			Dictionary<Index, List<GameObject>> meshes = new Dictionary<Index, List<GameObject>>();
			foreach (Transform child in transform) {
				VoxelMeshObject meshObject = child.GetComponent<VoxelMeshObject>();
				if (meshObject == null)
					continue;
				List<GameObject> objects;
				meshes.TryGetValue(meshObject.index, out objects);
				if (objects == null) {
					objects = new List<GameObject>();
					meshes[meshObject.index] = objects;
				}
				objects.Add(meshObject.gameObject);
			}
			return meshes;
		}



		// command operations

		public void initialize() {
			if (realHead == null)
				realHead = new VoxelBlock();
		}

		public void Update() {
			applyQueuedMeshes();
			if (jobQueue.Count < 1)
				generatingTrees.Remove(this);
			if (generationPaused) {
				if (VoxelThread.getJobCount() < 1 && jobQueue.Count < 1) {
					generationPaused = false;
					Time.timeScale = 1;
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

		public void wipe() {
			clearRenderers();
			if (head != null) {
				realHead = null;
			}
			dirty = true;
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
			enqueueCheck(new UpdateCheckJob(head, this, Index.ZERO));
		}

		public void relinkRenderers() {
			relinkRenderers(findRendererObjects());
		}

		private void relinkRenderers(Dictionary<Index, List<GameObject>> meshes) {
			lock(this) {
				foreach (Index index in meshes.Keys) {
					List<GameObject> objects = meshes[index];
					VoxelRenderer rend;
					renderers.TryGetValue(index, out rend);
					if (rend == null) {
						rend = new VoxelRenderer(index, this);
						renderers[index] = rend;
					}
					rend.obs = objects.ToArray();
				}
			}
		}

		public void OnBeforeSerialize() {
			lock (this) {
				if (voxelData.Length < 1 || dirty || head == null) {
					dirty = false;
					voxelData = new byte[0];
					if (head != null) {
						MemoryStream stream = new MemoryStream();
						BinaryWriter writer = new BinaryWriter(stream);
						head.serialize(writer);
						voxelData = stream.ToArray();
						stream.Close();
					}
				}
			}
		}

		public void OnAfterDeserialize() {
			lock (this) {
				if (voxelData.Length > 0) {
					initialize();
					MemoryStream stream = new MemoryStream(voxelData);
					BinaryReader reader = new BinaryReader(stream);
					realHead = (VoxelBlock)VoxelHolder.deserialize(reader);
					stream.Close();
				}

				// relink renderers
				enqueueJob(new LinkRenderersJob(this));
			}
		}

		public bool import(string fileName) {
			Stream stream = File.OpenRead(fileName);
			BinaryReader reader = new BinaryReader(stream);
			bool successful = read(reader);
			stream.Close();
			return successful;
		}

		public void export(string fileName) {
			if (head != null) {
				Stream stream = File.Create(fileName);
				BinaryWriter writer = new BinaryWriter(stream);
				writer.Write(FILE_FORMAT_VERSION);
				write(writer);
				stream.Close();
			}
		}

		public bool read(BinaryReader reader) {
			ulong fileFormatVersion = reader.ReadUInt64();
			// check format compatibility
			if (fileFormatVersion != FILE_FORMAT_VERSION
				&& fileFormatVersion != 1) {
				print("Wrong voxel data format version: " +fileFormatVersion +", should be " +FILE_FORMAT_VERSION);
				return false;
			} else {
				wipe();
				// read meta data
				if (fileFormatVersion > 1) {
					maxDepth = reader.ReadByte();
					int substanceCount = reader.ReadInt32();
					if (substanceCount > voxelSubstances.Length)
						System.Array.Resize(ref voxelSubstances, substanceCount);
				}

				// read voxel data
				realHead = (VoxelBlock)VoxelHolder.deserialize(reader);
				dirty = true;
				return true;
			}
		}

		public void write(BinaryWriter writer) {
			// write meta data
			writer.Write(maxDepth);
			writer.Write(voxelSubstances.Length);

			// write voxel data
			head.serialize(writer);
		}

		public void pauseForGeneration() {
			if (!Application.isPlaying)
				return;
			generationPaused = true;
			Time.timeScale = 0;
		}

		internal void addUpdateCheckJob() {
			++updateCheckJobs;
		}

		internal void removeUpdateCheckJob() {
			--updateCheckJobs;
		}

		internal void enqueueJob(VoxelJob job) {
			lock(this) {
				generatingTrees.Add(this);
				jobQueue.Enqueue(job);
			}
		}
	}

}
