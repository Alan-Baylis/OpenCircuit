using UnityEngine;

namespace Vox {

	public class BlurMutator : Mutator {

		public int blurRadius = 3;
		public bool overwriteSubstance = false;
		public float strength;
		public Vector3 worldPosition;
		public float worldRadius;

		public BlurMutator(Vector3 worldPosition, float worldRadius, float strength) {
			this.strength = strength;
			this.worldPosition = worldPosition;
			this.worldRadius = worldRadius;
		}

		public override App init(OcTree tree) {
			BlurApp app = new BlurApp(tree);
			float radius = worldRadius / tree.voxelSize;
			Vector3 radiusCube = new Vector3(radius, radius, radius);
			Vector3 center = tree.transform.InverseTransformPoint(worldPosition) / tree.voxelSize;
			Vector3 exactMin = center - radiusCube;
			Vector3 exactMax = center + radiusCube;
			app.min = new Index(tree.maxDepth, (uint)exactMin.x, (uint)exactMin.y, (uint)exactMin.z);
			app.max = new Index(tree.maxDepth, (uint)exactMax.x, (uint)exactMax.y, (uint)exactMax.z);
			app.minRadius = radius - 1;
			app.maxRadius = radius + 1;
			app.position = center;
			app.radius = radius;
			app.setOriginal();
			return app;
		}

		public override Act checkMutation(App application, Index pos) {
			BlurApp app = (BlurApp) application;
			Act act = new Act();
			ActCache actCache = new ActCache();
			act.cache = actCache;
			float voxelSize = calculateVoxelSize(app, pos);
			Vector3 diff = calculateDiff(app.position, pos, voxelSize);

			actCache.disSqr = diff.sqrMagnitude;
			float maxRadius = app.radius + voxelSize;
			float maxRadSqr = maxRadius * maxRadius;
			if (actCache.disSqr > maxRadSqr)
				return act;
			act.doTraverse = true;
			act.modify = true;
			return act;
		}

		public override Voxel mutate(App application, Index pos, Act act, Voxel original) {
			BlurApp app = (BlurApp) application;
			ActCache actCache = (ActCache) act.cache;
			float dis = Mathf.Sqrt(actCache.disSqr);
			float actualStrength = strength * (1 - dis / app.radius);
			if (actualStrength <= 0)
				return original;
			byte newOpacity = calculateOpacity(app.original, pos.x - app.min.x, pos.y - app.min.y, pos.z - app.min.z, actualStrength);
			return new Voxel(original.averageMaterialType(), newOpacity);
		}

		protected byte calculateOpacity(Voxel[,,] original, uint x, uint y, uint z, float strength) {
			double opacity = original[x, y, z].averageOpacity();
			int minX = Mathf.Max((int)x - blurRadius, 0);
			int maxX = Mathf.Min((int)x + blurRadius, original.GetLength(0));
			int minY = Mathf.Max((int)y - blurRadius, 0);
			int maxY = Mathf.Min((int)y + blurRadius, original.GetLength(1));
			int minZ = Mathf.Max((int)z - blurRadius, 0);
			int maxZ = Mathf.Min((int)z + blurRadius, original.GetLength(2));
			int count = 0;
			for (int xi = minX; xi < maxX; ++xi) {
				for (int yi = minY; yi < maxY; ++yi) {
					for (int zi = minZ; zi < maxZ; ++zi) {
						++count;
						Vector3 diff = new Vector3(x - xi, y - yi, z - zi);
						float dis = diff.magnitude;
						Voxel value = original[xi, yi, zi];
						if (dis < 0.5f || value == null)
							continue;
						float factor = Mathf.Min(Mathf.Max((1 - dis / blurRadius) * strength * 0.1f, 0), 1);
						opacity = opacity * (1 - factor) + value.averageOpacity() * factor;
					}
				}
			}
			return (byte)Mathf.Min((float)opacity, byte.MaxValue);
		}

		public class BlurApp : App {
			public Vector3 position;
			public float minRadius, maxRadius;
			public Voxel[,,] original;
			public float radius;

			public BlurApp(OcTree tree) : base(tree) { }

			public void setOriginal() {
				original = new ArrayReader(min, max.getNeighbor(1, 1, 1)).read(tree);
			}
		}

		public class ActCache {
			public float disSqr;
		}
	}
}