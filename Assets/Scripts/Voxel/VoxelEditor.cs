using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


namespace Vox {

	[AddComponentMenu("Scripts/Voxel/VoxelEditor")]
	[ExecuteInEditMode]
	public class VoxelEditor : OcTree {

		public const string DEFAULT_MATERIAL_PATH = "Assets/Materials/Voxel/VoxelBase.mat";
		public const string DEFAULT_BLEND_MATERIAL_PATH = "Assets/Materials/Voxel/VoxelBaseBlend.mat";
		public const string DEFAULT_PHYSICS_MATERIAL_PATH = "Assets/Materials/Voxel/Rock.physicMaterial";

		protected static Color brushGhostColor = new Color(0f, 1f, 0.4f, 1f);
		protected static Color brushGhostSubtractColor = new Color(1f, 0.2f, 0.2f, 1f);
		protected static Color brushGhostPaintColor = new Color(0f, 0.2f, 1f, 1f);

		public byte[] heightmapSubstances;
        public Texture2D[] heightmaps;
		public float maxChange;
        public int proceduralSeed;
        public float heightPercentage = 50;
		public float spherePercentage = 75;
        public bool gridEnabled = false;
        public bool gridUseVoxelUnits = false;
        public float gridSize = 1;
		public float maskDisplayAlpha = 0.3f;

        // editor data
		public int selectedMode = 0;
		public int selectedBrush = 0;
		public float sphereBrushSize = 1;
		public byte sphereBrushSubstance = 0;
		public bool sphereSubstanceOnly = false;
		public Vector3 cubeBrushDimensions = new Vector3(1, 1, 1);
		public byte cubeBrushSubstance = 0;
		public bool cubeSubstanceOnly = false;
		public float smoothBrushSize = 1;
		public float smoothBrushStrength = 1;
		public int smoothBrushBlurRadius = 3;
		public float ghostBrushAlpha = 0.3f;
		public Vector3[] pathPoints = null;
		public bool showPositionHandles = false;

		public void Start() {
			if (hasVoxelData && findRendererObjects().Count < 1) {
				generateRenderers();
				pauseForGeneration();
			}
		}

		public void setToHeightmap() {
			initialize();
			int dimension = heightmaps[0].height;

			for (int index = 0; index < heightmaps.Length; ++index ) {
				float[,] map = new float[dimension, dimension];
				for (int i = 0; i < heightmaps[index].height; i++) {
					for (int j = 0; j < heightmaps[index].width; j++) {
						Color pix = heightmaps[index].GetPixel((dimension - 1) - i, j);
						map[j, i] = ((pix.r + pix.g + pix.b) / 3.0f) * dimension;
					}
				}
				head.setToHeightmap(maxDepth, 0, 0, 0, ref map, heightmapSubstances[index], this);
			}
		}
		
		public void setToHeight() {
			initialize();
			Vector3 widthVector = new Vector3(width, width *heightPercentage /100, width);
            CubeMutator mut = new CubeMutator(transform.position +widthVector /2, widthVector, Voxel.full);
			mut.ignoreMasks = true;
			mut.apply(this);
		}
		
		public void setToSphere() {
			initialize();
			float radius = spherePercentage / 200f * width;
			float center = width /2f;
			SphereMutator mut = new SphereMutator(transform.TransformPoint(center, center, center), radius, new Voxel(0, byte.MaxValue));
			mut.ignoreMasks = true;
			mut.apply(this);
		}

		// this functions sets the values of the voxels, doing all of the procedural generation work
		// currently it just uses a "height map" system.  This is fine for initial generation, but
		// then more passes need to be done for cliffs, caves, streams, etc.
		public virtual void setToProcedural() {
			initialize();

			// the following generates terrain from a height map
			Random.seed = proceduralSeed;
			int dimension = 1 << maxDepth;
			float acceleration = 0;
			float height = dimension * 0.6f;
			float[,] heightMap = new float[dimension, dimension];
			float[,] accelMap = new float[dimension, dimension];
			byte[,] matMap = new byte[dimension, dimension];
			for (int x = 0; x < dimension; ++x) {
				for (int z = 0; z < dimension; ++z) {
					matMap[x, z] = 0;

					// calculate the height
					if (x != 0) {
						if (z == 0) {
							height = heightMap[x - 1, z];
							acceleration = accelMap[x - 1, z];
						} else {
							height = (heightMap[x - 1, z] + heightMap[x, z - 1]) / 2;
							acceleration = (accelMap[x - 1, z] + accelMap[x, z - 1]) / 2;
						}
					}
					float edgeDistance = Mathf.Max(Mathf.Abs(dimension / 2 - x - 10), Mathf.Abs(dimension / 2 - z - 10));
					float edgeDistancePercent = 1 - edgeDistance / (dimension / 2);
					float percent;
					if (edgeDistancePercent < 0.2)
						percent = height / (dimension * 0.6f) - 0.4f;
					else
						percent = height / (dimension * 0.4f);
					float roughness = maxChange + 0.2f * (1 - edgeDistancePercent);
					acceleration += Random.Range(-roughness * percent, roughness * (1 - percent));
					acceleration = Mathf.Min(Mathf.Max(acceleration, -roughness * 7), roughness * 7);
					height = Mathf.Min(Mathf.Max(height + acceleration, 0), dimension);
					heightMap[x, z] = height;
					accelMap[x, z] = acceleration;
				}
			}
			head.setToHeightmap(maxDepth, 0, 0, 0, ref heightMap, matMap, this);

			// generate trees
			//for (int x = 0; x < dimension; ++x) {
			//	for (int z = 0; z < dimension; ++z) {
			//		if (Random.Range(Mathf.Abs(accelMap[x, z]) / treeSlopeTolerance, 1) < treeDensity) {
			//			GameObject tree = (GameObject)GameObject.Instantiate(trees);
			//			tree.transform.parent = transform;
			//			tree.transform.localPosition = new Vector3(x * size, heightMap[x, z] * size - 1.5f, z * size);
			//			++treeCount;
			//		}
			//	}
			//}
		}

#if UNITY_EDITOR
		public static System.Nullable<Vector3> getRayCollision(Ray ray) {
			RaycastHit firstHit = new RaycastHit();
			firstHit.distance = float.PositiveInfinity;
			foreach(RaycastHit hit in Physics.RaycastAll(ray)) {
				if (hit.distance < firstHit.distance) {
					firstHit = hit;
				}
			}
			if (firstHit.distance == float.PositiveInfinity)
				return null;
			return firstHit.point;
		}

	    public System.Nullable<Vector3> getBrushPoint(Ray mouseLocation) {
			System.Nullable<Vector3> point = getRayCollision(mouseLocation);
	        if (point != null && gridEnabled) {
	            Vector3 actualPoint = transform.InverseTransformPoint(point.Value);
	            double halfGrid = gridSize / 2.0;
	            Vector3 mod = new Vector3(actualPoint.x %gridSize, actualPoint.y %gridSize, actualPoint.z %gridSize);
				actualPoint.x += (mod.x > halfGrid) ? gridSize -mod.x: -mod.x;
				actualPoint.y += (mod.y > halfGrid) ? gridSize -mod.y: -mod.y;
				actualPoint.z += (mod.z > halfGrid) ? gridSize -mod.z: -mod.z;
	            point = transform.TransformPoint(actualPoint);
	        }
	        return point;
	    }

		public void addPathPoint(Vector3 point) {
			if (pathPoints == null || pathPoints.Length < 1) {
				pathPoints = new Vector3[] { point };
			} else {
				System.Array.Resize(ref pathPoints, pathPoints.Length + 1);
				pathPoints[pathPoints.Length - 1] = point;
			}
		}

		public static VoxelEditor createEmpty() {
			GameObject ob = new GameObject();
			ob.name = "Voxel Object";
			VoxelEditor editor = ob.AddComponent<VoxelEditor>();
			editor.voxelSubstances = new VoxelSubstance[1];
			VoxelSubstance sub = new VoxelSubstance("Base",
				UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(DEFAULT_MATERIAL_PATH),
				UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(DEFAULT_BLEND_MATERIAL_PATH),
				UnityEditor.AssetDatabase.LoadAssetAtPath<PhysicMaterial>(DEFAULT_PHYSICS_MATERIAL_PATH));
			editor.voxelSubstances[0] = sub;
			return editor;
		}

		public bool isSubtracting() {
			return UnityEngine.Event.current.shift;
		}

		public bool isPathing() {
			return UnityEngine.Event.current.control && isSelectedBrushPathable();
		}

		public bool isSelectedBrushPathable() {
			return selectedBrush == 0 || selectedBrush == 1;
		}

		public void OnDrawGizmosSelected() {
			if (selectedMode == 0)
				return;
			if (ghostBrushAlpha > 0 && selectedMode == 1) {
				Ray mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(UnityEngine.Event.current.mousePosition);
				Color brushColor = getBrushColor();
				brushColor.a = ghostBrushAlpha;
				System.Nullable<Vector3> point = getBrushPoint(mouseRay);
				if (point != null) {
					Gizmos.color = brushColor;
					drawBrushGizmo(point.Value);
					if (isPathing()) {
						drawPathPoint(point.Value);
					}
				}

				// draw path points
				if (pathPoints != null && pathPoints.Length > 0 && isSelectedBrushPathable()) {
					for(int i=0; i<pathPoints.Length; ++i) {
						Gizmos.color = brushColor;
						drawBrushGizmo(pathPoints[i]);
						drawPathPoint(pathPoints[i]);
						if(showPositionHandles) {
							pathPoints[i] = UnityEditor.Handles.PositionHandle(pathPoints[i], Quaternion.identity);
						}
					}
					Gizmos.color = Color.yellow;
					for (int i = 0; i < pathPoints.Length -1; ++i) {
						Gizmos.DrawLine(pathPoints[i], pathPoints[i +1]);
					}
					if (point != null)
						Gizmos.DrawLine(pathPoints[pathPoints.Length -1], point.Value);
				}
			}
			if (maskDisplayAlpha > 0 && masks != null) {
				Gizmos.color = new Color(1, 0, 0, maskDisplayAlpha);
				foreach (VoxelMask mask in masks) {
					if (!mask.active)
						continue;
					Gizmos.DrawMesh(generateRectangleMesh(new Vector3(width, 0, width)), transform.TransformPoint(width / 2, mask.yPosition * voxelSize, width / 2));
				}
				Gizmos.color = Color.gray;
			}
		}

		protected void drawBrushGizmo(Vector3 location) {
			switch (selectedBrush) {
				case 0:
					Gizmos.DrawSphere(location, sphereBrushSize);
					break;
				case 1:
					Gizmos.DrawMesh(generateRectangleMesh(cubeBrushDimensions), location);
					break;
				case 2:
					Gizmos.DrawSphere(location, smoothBrushSize);
					break;
			}
		}

		protected void drawPathPoint(Vector3 point) {
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(point, new Vector3(0.5f, 0.5f, 0.5f) *voxelSize);
		}

		protected Color getBrushColor() {
			switch (selectedBrush) {
				case 0:
					if (sphereSubstanceOnly)
						return brushGhostPaintColor;
					return isSubtracting() ? brushGhostSubtractColor : brushGhostColor;
				case 1:
					if (cubeSubstanceOnly)
						return brushGhostPaintColor;
					return isSubtracting() ? brushGhostSubtractColor : brushGhostColor;
			}
			return brushGhostColor;
		}


		protected Mesh generateRectangleMesh(Vector3 scale) {
			Mesh mesh = new Mesh();
			scale = scale / 2;
			Vector3[] vertices = new Vector3[] {
				new Vector3(-scale.x, -scale.y, -scale.z),
				new Vector3( scale.x, -scale.y, -scale.z),
				new Vector3(-scale.x,  scale.y, -scale.z),
				new Vector3( scale.x,  scale.y, -scale.z),
				new Vector3(-scale.x, -scale.y,  scale.z),
				new Vector3( scale.x, -scale.y,  scale.z),
				new Vector3(-scale.x,  scale.y,  scale.z),
				new Vector3( scale.x,  scale.y,  scale.z),
			};
			mesh.vertices = vertices;
			mesh.normals = new Vector3[vertices.Length];
			mesh.triangles = new int[] {
				0, 1, 5,
				5, 4, 0,
				2, 7, 3,
				7, 2, 6,
				0, 3, 1,
				2, 3, 0,
				4, 5, 7,
				6, 4, 7,
				1, 3, 5,
				5, 3, 7,
				0, 4, 2,
				4, 6, 2,
			};
			return mesh;
		}
#endif

	}
}
