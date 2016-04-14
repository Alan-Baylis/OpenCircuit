using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Vox {

	[ExecuteInEditMode]
	[System.Serializable]
	public class VoxelRenderer {

		public static int rendCount = 0;
		public static int duplicateTriangleCount = 0;

		public const byte VOXEL_COUNT_POWER = 4;
		public const byte VOXEL_DIMENSION = 1 << VOXEL_COUNT_POWER;
		public const byte VERTEX_DIMENSION = VOXEL_DIMENSION + 1;
		public const float NORMAL_SMOOTHNESS = 0.1f; // 0 - 1

		public Dictionary<int, int> vertices;
		public Dictionary<int, byte> vertexSubstances;
		[System.NonSerialized]
		public Tree control;
//		public MeshCollider collider;
		public float size;
		public Vector3 position;
//		public byte xDim, yDim, zDim;
//		public byte xExtend, yExtend, zExtend;
		public GameObject[] obs;
		public Vector3[] VERTS, NORMS;
		public int[] TRIS;
		public bool applied = false;
		public bool old = false;
		public Index index;


		public void clear() {
			//			lock (this) {
			//MonoBehaviour.print("Deleted Renderer");
				if (control != null) {
					lock(control) {
						control.renderers.Remove(index);
					}
				}
				removePolyCount();
				if (obs != null)
					foreach (GameObject ob in obs) {
						GameObject.DestroyImmediate(ob);
					}
//				if (collider != null)
//					GameObject.DestroyImmediate(collider);
//			}
		}

		public VoxelRenderer(Index index, Tree control):
			this(index, control, new Vector3(
				index.x * control.sizes[index.depth],
				index.y * control.sizes[index.depth],
				index.z * control.sizes[index.depth])) {
		}

		public VoxelRenderer(Index index, Tree control, Vector3 localPosition) {
			this.index = index;
			this.position = localPosition;
			this.control = control;
			size = 0;
			++rendCount;
			vertices = new Dictionary<int, int>();
			VERTS = new Vector3[0];
			NORMS = new Vector3[0];
			TRIS = new int[0];
			lock(control) {
				control.renderers[index] = this;
			}
		}

		public void genMesh(Voxel[,,] voxels) {
			if (control == null)
				return;

			size = control.sizes[index.depth];

			Queue<int[]> triangleSet = new Queue<int[]>();
			Dictionary<int, Vector3> actualVertices = new Dictionary<int, Vector3>();
			vertexSubstances = new Dictionary<int, byte>();
			lock (control) {
			//lock (typeof(MarchingCubes)) {
				MarchingCubes.setup(size / VOXEL_DIMENSION, control.isoLevel, ref actualVertices, ref vertexSubstances, ref voxels, position + new Vector3(0.5f, 0.5f, 0.5f) * size / VOXEL_DIMENSION, null);
				int totalTris = 0;
				uint xDim = (uint)voxels.GetLength(0) -1;
				uint yDim = (uint)voxels.GetLength(1) -1;
				uint zDim = (uint)voxels.GetLength(2) -1;

				for (int x = 0; x < xDim; ++x) {
					for (int y = 0; y < yDim; ++y) {
						for (int z = 0; z < zDim; ++z) {
							int[] tris = MarchingCubes.lookupTriangles(x, y, z, x+1, y+1, z+1);
							if (tris == null) continue;
							triangleSet.Enqueue(tris);
							totalTris += tris.Length;
						}
					}
				}
			}

			if (actualVertices.Count < 1) {
				applied = true;
				return;
			}

			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			watch.Start();


			List<int> triangles = new List<int>();
			List<Vector3> finalVertices = new List<Vector3>(actualVertices.Count);
			vertices.Clear();
			while (triangleSet.Count > 0) {
				int[] triangleList = triangleSet.Dequeue();
				for (int i = 0; i < triangleList.Length; ++i) {
					if (!vertices.ContainsKey(triangleList[i])) {
						finalVertices.Add(actualVertices[triangleList[i]]);
						vertices[triangleList[i]] = finalVertices.Count - 1;
					}
					triangles.Add(vertices[triangleList[i]]);
				}
			}
			VERTS = finalVertices.ToArray();
			TRIS = triangles.ToArray();
			calcNorms();

			alignEdge(0, 1, 1);
			alignEdge(1, 0, 1);
			alignEdge(1, 1, 0);
			lock (control) {
				control.enqueueJob(new ApplyMeshJob(this, index.depth, index.x, index.y, index.z));
			}

			watch.Stop();
			control.meshGenArrayTime += watch.Elapsed.TotalSeconds;

		}

		public void applyMesh() {
			applied = true;
			if (TRIS.Length < 1 && (obs == null || obs.Length < 1))
				return;

			// convert the vertexSubstances structure into a more directly usable format
			byte[] substanceToVertices = new byte[VERTS.Length];
			foreach (int index in vertices.Keys) {
				if (vertexSubstances.ContainsKey(index)) {
					byte substance = vertexSubstances[index];
					substanceToVertices[vertices[index]] = substance;
				}
			}
			
			// build triangle and vertex lists for each mesh from the master triangle list
			Dictionary<SubstanceCollection, Dictionary<int, int>> substanceVertices = new Dictionary<SubstanceCollection, Dictionary<int, int>>();
			Dictionary<SubstanceCollection, List<int>> substanceTriangles = new Dictionary<SubstanceCollection, List<int>>();
			for(int i=0; i<TRIS.Length; i+=3) {
				SubstanceCollection subs = new SubstanceCollection();
				for(int j=0; j<3; ++j) {
					byte sub = substanceToVertices[TRIS[i +j]];
					subs.add(sub);
				}
				if (!substanceTriangles.ContainsKey(subs)) {
					substanceTriangles[subs] = new List<int>(TRIS.Length /substanceToVertices.Length);
					substanceVertices[subs] = new Dictionary<int, int>();
				}
				List<int> specificSubstanceTriangles = substanceTriangles[subs];
				Dictionary<int, int> specificSubstanceVertexIndices = substanceVertices[subs];
				for(int j=0; j<3; ++j) {
					int vertexIndex = TRIS[i +j];
					if (!specificSubstanceVertexIndices.ContainsKey(vertexIndex))
						specificSubstanceVertexIndices[vertexIndex] = specificSubstanceVertexIndices.Count;
					specificSubstanceTriangles.Add(specificSubstanceVertexIndices[vertexIndex]);
				}
			}

			// create and initialize the game objects which will have the mesh renderers and colliders attached to them
			removePolyCount();
			GameObject[] oldObs = (obs == null)? new GameObject[0]: obs;
			obs = new GameObject[substanceTriangles.Count];
			if (oldObs.Length > obs.Length) {
				Array.Copy(oldObs, obs, obs.Length);
				for (int i = obs.Length; i < oldObs.Length; ++i) {
					GameObject.DestroyImmediate(oldObs[i]);
				}
			} else {
				Array.Copy(oldObs, obs, oldObs.Length);
				for(int i=oldObs.Length; i<obs.Length; ++i) {
					obs[i] = createRendererGameObject();
				}
			}
			foreach(GameObject ob in obs) {
				foreach(MeshCollider col in ob.GetComponents<MeshCollider>())
					GameObject.DestroyImmediate(col);
			}

			// Assign vertex data to the game object meshes
			int obIndex = 0;
			foreach (SubstanceCollection substances in substanceTriangles.Keys) {
				assignMesh(obs[obIndex], substances, substanceVertices[substances], substanceTriangles[substances], substanceToVertices);
				++obIndex;
			}

			//			// refresh collider
			//			if (control.createColliders) {
			//				collider.enabled = false;
			//				if (VoxelBlock.isRenderSize(size, control))
			//					collider.enabled = true;
			//			}
			addPolyCount();
		}

		protected GameObject createRendererGameObject() {
			GameObject gameObject = new GameObject("Voxel Section");
			gameObject.isStatic = control.useStaticMeshes;
			if (!control.saveMeshes)
				gameObject.hideFlags |= HideFlags.DontSave;
			Transform t = gameObject.transform;
			t.parent = control.transform;
			t.localPosition = Vector3.zero;
			t.hideFlags |= HideFlags.HideInHierarchy;
			MeshRenderer rend = gameObject.AddComponent<MeshRenderer>();
			rend.enabled = false;
			rend.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbesAndSkybox;
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetSelectedWireframeHidden(rend, true);
#endif
			gameObject.AddComponent<MeshFilter>().sharedMesh = new Mesh();
			gameObject.AddComponent<VoxelMeshObject>().index = index;
			return gameObject;
		}

		protected void assignMesh(GameObject meshObject, SubstanceCollection substances, Dictionary<int, int> vertices, List<int> triangles, byte[] MATS) {
			byte[] substanceArray = substances.getSubstances();
			bool hasGrass = substanceArray.Length == 1 && control.voxelSubstances[substanceArray[0]].grassMaterial != null;
			int vertexCount = vertices.Count;
			if (hasGrass)
				vertexCount *= 2;
			Vector3[] verts = new Vector3[vertexCount];
			Vector3[] norms = new Vector3[vertexCount];
			Vector2[] uvs = new Vector2[vertexCount];

			// create the vertex, normal, and uv arrays
			foreach (int index in vertices.Keys) {
				int i = vertices[index];
				norms[i] = NORMS[index];
				verts[i] = VERTS[index];
				switch(substances.getSubstanceRelativeIndex(MATS[index])) {
				case 0:
					uvs[i] = Vector2.zero;
					break;
				case 1:
					uvs[i] = Vector2.right;
					break;
				case 2:
					uvs[i] = Vector2.up;
					break;
				}
			}
			if (hasGrass) {
				VoxelSubstance substance = control.voxelSubstances[substanceArray[0]];
				for (int i = vertices.Count; i<vertexCount; ++i) {
					int index = i -vertices.Count;
					norms[i] = norms[index];
					verts[i] = verts[index];
					if (norms[i].y > substance.grassMinFlatness) {
						float factor = (norms[i].y -substance.grassMinFlatness +0.1f) /(1 - substance.grassMinFlatness +0.1f);
						verts[i].y += substance.grassHeight *factor;
						uvs[i] = new Vector2(0, 1 -factor);
					} else {
						uvs[i] = Vector2.up;
					}
				}
			}

			// apply the render materials to the renderer
			MeshRenderer rend = meshObject.GetComponent<MeshRenderer>();
			PhysicMaterial phyMat = null;
			if (substanceArray.Length == 1) {
				Material[] materials = new Material[1];
                if (hasGrass) {
					materials = new Material[2];
					materials[1] = control.voxelSubstances[substanceArray[0]].grassMaterial;
                }
				materials[0] = control.voxelSubstances[substanceArray[0]].renderMaterial;
				//materials[0].EnableKeyword("IS_BASE");
				rend.sharedMaterials = materials;
				phyMat = control.voxelSubstances[substanceArray[0]].physicsMaterial;
			} else {
				Material[] materials = new Material[substanceArray.Length];
				for(int i=0; i<materials.Length; ++i) {
					Material material = new Material(control.voxelSubstances[substanceArray[i]].blendMaterial);
					material.renderQueue = i;
					foreach (string keyword in material.shaderKeywords)
						material.DisableKeyword(keyword);
					if (!control.saveMeshes)
						material.hideFlags = HideFlags.HideAndDontSave;
					switch(i) {
					case 0:
						material.EnableKeyword("IS_BASE");
						phyMat = control.voxelSubstances[substanceArray[i]].physicsMaterial;
						break;
					case 1:
						material.EnableKeyword("IS_X");
						break;
					case 2:
						material.EnableKeyword("IS_Y");
						break;
					}
					materials[i] = material;
				}
				rend.materials = materials;
			}
			
			Mesh m = meshObject.GetComponent<MeshFilter>().sharedMesh;
			m.Clear();
			int[] triangleArray = triangles.ToArray();

			// reduce mesh
			if (control.reduceMeshes) {
				HashSet<int> verticesRemoved = VoxelMeshReducer.reduce(ref verts, ref triangleArray, control.reductionAmount);
				norms = VoxelMeshReducer.removeEntries(norms, verticesRemoved);
				uvs = VoxelMeshReducer.removeEntries(uvs, verticesRemoved);
			}

			m.vertices = verts;
			m.normals = norms;
			m.uv = uvs;

			if (hasGrass) {
				m.subMeshCount = 2;
				int[] grassTriangles = new int[triangleArray.Length];
				for (int i = 0; i<grassTriangles.Length; ++i)
					grassTriangles[i] = triangleArray[i] +vertices.Count;
				m.SetTriangles(grassTriangles, 1);
			} else {
				m.subMeshCount = 1;
			}

			m.SetTriangles(triangleArray, 0);
			m.RecalculateBounds();
			m.Optimize();
			rend.enabled = true;

			// add a collider for the mesh
			if (control.createColliders) {
				MeshCollider collider = meshObject.AddComponent<MeshCollider>();
				collider.material = phyMat;
				if (hasGrass) {
					Mesh mesh = new Mesh();
					Vector3[] colVerts = new Vector3[vertices.Count];
					Vector3[] colNorms = new Vector3[vertices.Count];
					Array.Copy(verts, colVerts, colVerts.Length);
					Array.Copy(norms, colNorms, colNorms.Length);
					mesh.vertices = colVerts;
					mesh.normals = colNorms;
					mesh.SetTriangles(triangles, 0);
					mesh.RecalculateBounds();
					mesh.Optimize();
					collider.sharedMesh = mesh;
				} else {
					collider.sharedMesh = m;
				}
//				collider.hideFlags = /*HideFlags.HideInInspector | */HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
			}
		}

		public void setupMeshes() {
#if UNITY_EDITOR
			foreach(GameObject ob in obs) {
				foreach(Renderer rend in ob.GetComponents<Renderer>()) {
					UnityEditor.EditorUtility.SetSelectedWireframeHidden(rend, true);
				}
			}
#endif
		}

		public float getSize() {
			return size;
		}

		private void alignEdge(byte x, byte y, byte z) {
			VoxelRenderer neighbor = control.getRenderer(index.getNeighbor(x -1, y -1, z -1));
			if (neighbor == null)
				return;
			
			lock (control) {
				if (x != 1) {
					int otherXInd = VERTEX_DIMENSION-1;
					int myXInd = 0;
					for (int yi = 0; yi < VERTEX_DIMENSION; ++yi) {
						for (int zi = 0; zi < VERTEX_DIMENSION; ++zi) {
							mergeNormals(neighbor, getY(myXInd, yi, zi), getY(otherXInd, yi, zi));
                            mergeNormals(neighbor, getZ(myXInd, yi, zi), getZ(otherXInd, yi, zi));
						}
					}
				} else if (y != 1) {
					int otherYInd = VERTEX_DIMENSION-1;
					int myYInd = 0;
					for (int xi = 0; xi < VERTEX_DIMENSION; ++xi) {
						for (int zi = 0; zi < VERTEX_DIMENSION; ++zi) {
							mergeNormals(neighbor, getX(xi, myYInd, zi), getX(xi, otherYInd, zi));
                            mergeNormals(neighbor, getZ(xi, myYInd, zi), getZ(xi, otherYInd, zi));
						}
					}
				} else if (z != 1) {
					int otherZInd = VERTEX_DIMENSION -1;
					int myZInd = 0;
					for (int xi = 0; xi < VERTEX_DIMENSION; ++xi) {
						for (int yi = 0; yi < VERTEX_DIMENSION; ++yi) {
							mergeNormals(neighbor, getX(xi, yi, myZInd), getX(xi, yi, otherZInd));
							mergeNormals(neighbor, getY(xi, yi, myZInd), getY(xi, yi, otherZInd));
						}
					}
				}
			}
		}

		private bool mergeNormals(VoxelRenderer neighbor, int myIndex, int otherIndex) {
			if (vertices.ContainsKey(myIndex) && neighbor.vertices.ContainsKey(otherIndex)) {
				NORMS[vertices[myIndex]] = neighbor.NORMS[neighbor.vertices[otherIndex]];
				return true;
			}
			return false;
		}

		private Vector3 getAverageNormal(Vector3 normal1, Vector3 normal2) {
			return (normal2 +normal1).normalized;
		}

//		public void addEdge(VoxelUpdateInfo info, byte x, byte y, byte z) {
//			if (vertices == null) return;
//			bool recalculate = false;
//			Voxel[, ,] voxels = new Voxel[VERTEX_DIMENSION, VERTEX_DIMENSION, VERTEX_DIMENSION];
//			if (x == 0/* && xExtend == 0*/) {
//				recalculate = true;
//				xExtend = 1;
//				for (byte yi = (byte)(1 - yExtend); yi < yDim; ++yi) {
//					for (byte zi = (byte)(1 - zExtend); zi < zDim; ++zi) {
//						voxels[0, yi, zi] = info.getSub(VOXEL_COUNT_POWER, (uint)(VOXEL_DIMENSION - xExtend), (uint)(VOXEL_DIMENSION - 1 + yi), (uint)(VOXEL_DIMENSION - 1 + zi)).toVoxel();
//						voxels[1, yi, zi] = info.getSub(VOXEL_COUNT_POWER, VOXEL_DIMENSION, (uint)(VOXEL_DIMENSION - 1 + yi), (uint)(VOXEL_DIMENSION - 1 + zi)).toVoxel();
//					}
//				}
//			} else if (x == 2/* && xDim < VERTEX_DIMENSION*/) {
//				recalculate = true;
//				xDim = VERTEX_DIMENSION;
//				for (byte yi = (byte)(1 - yExtend); yi < yDim; ++yi) {
//					for (byte zi = (byte)(1 - zExtend); zi < zDim; ++zi) {
//						voxels[VOXEL_DIMENSION + 1, yi, zi] = info.getSub(VOXEL_COUNT_POWER, VOXEL_DIMENSION * 2, (uint)(VOXEL_DIMENSION - 1 + yi), (uint)(VOXEL_DIMENSION - 1 + zi)).toVoxel();
//						voxels[VOXEL_DIMENSION, yi, zi] = info.getSub(VOXEL_COUNT_POWER, VOXEL_DIMENSION * 2 - 1, (uint)(VOXEL_DIMENSION - 1 + yi), (uint)(VOXEL_DIMENSION - 1 + zi)).toVoxel();
//					}
//				}
//			} else if (y == 0/* && yExtend == 0*/) {
//				recalculate = true;
//				yExtend = 1;
//				for (byte xi = (byte)(1 - xExtend); xi < xDim; ++xi) {
//					for (byte zi = (byte)(1 - zExtend); zi < zDim; ++zi) {
//						voxels[xi, 0, zi] = info.getSub(VOXEL_COUNT_POWER, (uint)(VOXEL_DIMENSION - 1 + xi), (uint)(VOXEL_DIMENSION - yExtend), (uint)(VOXEL_DIMENSION - 1 + zi)).toVoxel();
//						voxels[xi, 1, zi] = info.getSub(VOXEL_COUNT_POWER, (uint)(VOXEL_DIMENSION - 1 + xi), VOXEL_DIMENSION, (uint)(VOXEL_DIMENSION - 1 + zi)).toVoxel();
//					}
//				}
//			} else if (y == 2/* && yDim < VERTEX_DIMENSION*/) {
//				recalculate = true;
//				yDim = VERTEX_DIMENSION;
//				for (byte xi = (byte)(1 - xExtend); xi < xDim; ++xi) {
//					for (byte zi = (byte)(1 - zExtend); zi < zDim; ++zi) {
//						voxels[xi, VOXEL_DIMENSION + 1, zi] = info.getSub(VOXEL_COUNT_POWER, (uint)(VOXEL_DIMENSION - 1 + xi), VOXEL_DIMENSION * 2, (uint)(VOXEL_DIMENSION - 1 + zi)).toVoxel();
//						voxels[xi, VOXEL_DIMENSION, zi] = info.getSub(VOXEL_COUNT_POWER, (uint)(VOXEL_DIMENSION - 1 + xi), VOXEL_DIMENSION * 2 - 1, (uint)(VOXEL_DIMENSION - 1 + zi)).toVoxel();
//					}
//				}
//			} else if (z == 0/* && zExtend == 0*/) {
//				recalculate = true;
//				zExtend = 1;
//				for (byte xi = (byte)(1 - xExtend); xi < xDim; ++xi) {
//					for (byte yi = (byte)(1 - yExtend); yi < yDim; ++yi) {
//						voxels[xi, yi, 0] = info.getSub(VOXEL_COUNT_POWER, (uint)(VOXEL_DIMENSION - 1 + xi), (uint)(VOXEL_DIMENSION - 1 + yi), (uint)(VOXEL_DIMENSION - zExtend)).toVoxel();
//						voxels[xi, yi, 1] = info.getSub(VOXEL_COUNT_POWER, (uint)(VOXEL_DIMENSION - 1 + xi), (uint)(VOXEL_DIMENSION - 1 + yi), VOXEL_DIMENSION).toVoxel();
//					}
//				}
//			} else if (z == 2/* && zDim < VERTEX_DIMENSION*/) {
//				recalculate = true;
//				zDim = VERTEX_DIMENSION;
//				for (byte xi = (byte)(1 - xExtend); xi < xDim; ++xi) {
//					for (byte yi = (byte)(1 - yExtend); yi < yDim; ++yi) {
//						voxels[xi, yi, VOXEL_DIMENSION + 1] = info.getSub(VOXEL_COUNT_POWER, (uint)(VOXEL_DIMENSION - 1 + xi), (uint)(VOXEL_DIMENSION - 1 + yi), VOXEL_DIMENSION * 2).toVoxel();
//						voxels[xi, yi, VOXEL_DIMENSION] = info.getSub(VOXEL_COUNT_POWER, (uint)(VOXEL_DIMENSION - 1 + xi), (uint)(VOXEL_DIMENSION - 1 + yi), VOXEL_DIMENSION * 2 - 1).toVoxel();
//					}
//				}
//			}
//
//			if (recalculate) {
//
//
//				Queue<int[]> triangleSet = new Queue<int[]>();
//				MarchingCubes.setup(info.size / VOXEL_DIMENSION, control.isoLevel, ref vertices, ref vertexSubstances, ref voxels, position - new Vector3(0.5f, 0.5f, 0.5f) * size / VOXEL_DIMENSION, VERTS);
//
//				byte xStart = (byte)(1 - xExtend + (VOXEL_DIMENSION + xExtend - 1) * (x / 2));
//				byte xEnd = (byte)(2 + (xDim - 2) * ((x + 1) / 2));
//				byte yStart = (byte)(1 - yExtend + (VOXEL_DIMENSION + yExtend - 1) * (y / 2));
//				byte yEnd = (byte)(2 + (yDim - 2) * ((y + 1) / 2));
//				byte zStart = (byte)(1 - zExtend + (VOXEL_DIMENSION + zExtend - 1) * (z / 2));
//				byte zEnd = (byte)(2 + (zDim - 2) * ((z + 1) / 2));
//
//				for (byte xi = xStart, x1 = (byte)(xi + 1); x1 < xEnd; xi = x1++) {
//					for (byte yi = yStart, y1 = (byte)(yi + 1); y1 < yEnd; yi = y1++) {
//						for (byte zi = zStart, z1 = (byte)(zi + 1); z1 < zEnd; zi = z1++) {
//							int[] tris = MarchingCubes.lookupTriangles(xi, yi, zi, x1, y1, z1);
//							if (tris == null) continue;
//							triangleSet.Enqueue(tris);
//						}
//					}
//				}
//
//				if (vertices.Count < 1) {
//					return;
//				}
//
//
//				List<int> newTriangles = new List<int>(TRIS);
//				List<Vector3> newVertices = new List<Vector3>(VERTS);
//				int tri = 0;
//				while (triangleSet.Count > 0) {
//					int[] triangleList = triangleSet.Dequeue();
//					for (int i = 0; i < triangleList.Length; ++i) {
//						if (vertices[triangleList[i]].GetType() == typeof(Vector3)) {
//							newVertices.Add((Vector3)vertices[triangleList[i]]);
//							vertices[triangleList[i]] = newVertices.Count - 1;
//						}
//						newTriangles.Add((int)vertices[triangleList[i]]);
//					}
//					tri += triangleList.Length;
//				}
//
//				Vector3[] finalNorms = new Vector3[newVertices.Count];
//				Array.Copy(NORMS, finalNorms, NORMS.Length);
//				int oldNormCount = NORMS.Length;
//
//				VERTS = newVertices.ToArray();
//				TRIS = newTriangles.ToArray();
//				calcNorms();
//				Array.Copy(NORMS, oldNormCount, finalNorms, oldNormCount, finalNorms.Length - oldNormCount);
//				NORMS = finalNorms;
//			}
//
//			alignEdge(info, x, y, z);
//			control.enqueueJob(new ApplyMeshJob(this, info.detailLevel, info.x, info.y, info.z));
//		}

		private static void addDualVertices(LinkedList<Vector3[]> otherVerts, VoxelRenderer rend, int x, int y, int z, byte xyz) {
			Vector3[] otherVert1;
			Vector3[] otherVert2;
			switch (xyz) {
				case 0:
					otherVert1 = getVertex(rend, getY(x, y, z));
					otherVert2 = getVertex(rend, getZ(x, y, z));
					break;
				case 1:
					otherVert1 = getVertex(rend, getX(x, y, z));
					otherVert2 = getVertex(rend, getZ(x, y, z));
					break;
				case 2:
					otherVert1 = getVertex(rend, getX(x, y, z));
					otherVert2 = getVertex(rend, getY(x, y, z));
					break;
				default:
					return;
			}
			if (otherVert1 != null) otherVerts.AddFirst(otherVert1);
			if (otherVert2 != null) otherVerts.AddFirst(otherVert2);
		}

		private static Vector3[] getVertex(VoxelRenderer rend, int hashIndex) {
			if (rend == null || rend.vertices == null) return null;
			if (!rend.vertices.ContainsKey(hashIndex)) return null;
			object index = rend.vertices[hashIndex];
			return new Vector3[] {
				rend.VERTS[(int)index],
				rend.NORMS[(int)index]
			};
		}

		private Vector3[] getClosestVertex(Vector3 position, LinkedList<Vector3[]> otherVertices) {
			Vector3[] closest = null;
			float closestDis = float.MaxValue;
			foreach (Vector3[] otherVertex in otherVertices) {
				float newDis = (position - otherVertex[0]).sqrMagnitude;
				if (closestDis > newDis) {
					closest = otherVertex;
					closestDis = newDis;
				}
			}
			return closest;
		}

		public static int getX(int x, int y, int z) {
			return (y * VERTEX_DIMENSION + z) * VERTEX_DIMENSION + x;
		}

		public static int getY(int x, int y, int z) {
			return ((VERTEX_DIMENSION + x) * VERTEX_DIMENSION + z) * VERTEX_DIMENSION + y;
		}

		public static int getZ(int x, int y, int z) {
			return ((VERTEX_DIMENSION * 2 + x) * VERTEX_DIMENSION + y) * VERTEX_DIMENSION + z;
		}

		//private static int getX(int index, int dimension) {
		//	return (y * VERTEX_DIMENSION + z) * VERTEX_DIMENSION + x;
		//}

		//private static int getY(int index, int dimension) {
		//	return ((VERTEX_DIMENSION + x) * VERTEX_DIMENSION + z) * VERTEX_DIMENSION + y;
		//}

		//private static int getZ(int index, int dimension) {
		//	return ((VERTEX_DIMENSION * 2 + x) * VERTEX_DIMENSION + y) * VERTEX_DIMENSION + z;
		//}

		//private static int getDimension(int index) {
		//	return index / (VERTEX_DIMENSION * VERTEX_DIMENSION * VERTEX_DIMENSION);
		//}

//		private Voxel[, ,] createVoxelArray(VoxelUpdateInfo info) {
////			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
////			watch.Start();
//			setDimensions(info);
//			Voxel[, ,] voxels = new Voxel[VERTEX_DIMENSION, VERTEX_DIMENSION, VERTEX_DIMENSION];
//
//			for (byte y = (byte)(1 - yExtend); y < yDim; ++y) {
//				for (byte z = (byte)(1 - zExtend); z < zDim; ++z) {
//					voxels[1 - xExtend, y, z] = info.getSub(VOXEL_COUNT_POWER, (uint)(VOXEL_DIMENSION - xExtend), (uint)(VOXEL_DIMENSION - 1 + y), (uint)(VOXEL_DIMENSION - 1 + z)).toVoxel();
//				}
//			}
//			for (byte x = (byte)(2 - xExtend); x < xDim; ++x) {
//				for (byte z = (byte)(1 - zExtend); z < zDim; ++z) {
//					voxels[x, 1 - yExtend, z] = info.getSub(VOXEL_COUNT_POWER, (uint)(VOXEL_DIMENSION - 1 + x), (uint)(VOXEL_DIMENSION - yExtend), (uint)(VOXEL_DIMENSION - 1 + z)).toVoxel();
//				}
//			}
//			for (byte x = (byte)(2 - xExtend); x < xDim; ++x) {
//				for (byte y = (byte)(2 - yExtend); y < yDim; ++y) {
//					voxels[x, y, 1 - zExtend] = info.getSub(VOXEL_COUNT_POWER, (uint)(VOXEL_DIMENSION - 1 + x), (uint)(VOXEL_DIMENSION - 1 + y), (uint)(VOXEL_DIMENSION - zExtend)).toVoxel();
//				}
//			}
////			watch.Stop();
////			control.meshGenArrayTime += watch.Elapsed.TotalSeconds;
//			return voxels;
//		}

//		private void setDimensions(VoxelUpdateInfo info) {
//			if (info.renderers[0, 1, 1] == null)
//				xExtend = 0;
//			else
//				xExtend = 1;
//			if (info.renderers[1, 0, 1] == null)
//				yExtend = 0;
//			else
//				yExtend = 1;
//			if (info.renderers[1, 1, 0] == null)
//				zExtend = 0;
//			else
//				zExtend = 1;
//			xDim = (byte)(VOXEL_DIMENSION + 1);
//			yDim = (byte)(VOXEL_DIMENSION + 1);
//			zDim = (byte)(VOXEL_DIMENSION + 1);
//			if (info.renderers[2, 1, 1] != null && info.renderers[2, 1, 1].size > size * 1.1f)
//				++xDim;
//			if (info.renderers[1, 2, 1] != null && info.renderers[1, 2, 1].size > size * 1.1f)
//				++yDim;
//			if (info.renderers[1, 1, 2] != null && info.renderers[1, 1, 2].size > size * 1.1f)
//				++zDim;
//		}

		private void calcNorms() {
			Vector3[] norms = new Vector3[VERTS.Length];
			for(int i=0; i<norms.Length; ++i) {
				norms[i] = Vector3.zero;
			}
			for (int i = 0; i < TRIS.Length;) {
				int A = TRIS[i++];
				int B = TRIS[i++];
				int C = TRIS[i++];
				Vector3 surfNorm = Vector3.Cross(VERTS[B] - VERTS[A], VERTS[C] - VERTS[A]);
				surfNorm = surfNorm * (1 - NORMAL_SMOOTHNESS + NORMAL_SMOOTHNESS / Mathf.Max(surfNorm.sqrMagnitude, 0.00000001f));
				norms[A] += surfNorm;
				norms[B] += surfNorm;
				norms[C] += surfNorm;
			}
			for (int i = 0; i < norms.Length; ++i) {
				norms[i].Normalize();
			}
			NORMS = norms;
		}

		public Tree getControl() {
			return control;
		}

		private void removePolyCount() {
			lock (control) {
				if (obs != null) {
					foreach (GameObject ob in obs) {
						control.triangleCount -= ob.GetComponent<MeshFilter>().sharedMesh.triangles.Length / 3;
						control.vertexCount -= ob.GetComponent<MeshFilter>().sharedMesh.vertexCount;
					}
				}
			}
		}

		private void addPolyCount() {
			lock (control) {
				if (obs != null) {
					foreach (GameObject ob in obs) {
						control.triangleCount += ob.GetComponent<MeshFilter>().sharedMesh.triangles.Length / 3;
						control.vertexCount += ob.GetComponent<MeshFilter>().sharedMesh.vertexCount;
					}
				}
			}
		}
	}

}