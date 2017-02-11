using UnityEngine;
using System.Collections;

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

		public override Application setup(OcTree target) {
			float radius = worldRadius / target.voxelSize;
			Vector3 radiusCube = new Vector3(radius, radius, radius);
			Vector3 center = target.transform.InverseTransformPoint(worldPosition) / target.voxelSize;
			Vector3 exactMin = center - radiusCube;
			Vector3 exactMax = center + radiusCube;
			BlurApp app = new BlurApp();
			app.tree = target;
			app.min = new Index(target.maxDepth, (uint)exactMin.x, (uint)exactMin.y, (uint)exactMin.z);
			app.max = new Index(target.maxDepth, (uint)exactMax.x, (uint)exactMax.y, (uint)exactMax.z);
			app.minRadius = radius - 1;
			app.maxRadius = radius + 1;
			app.position = center;
			app.radius = radius;
			app.setOriginal(target);
			return app;
		}

		protected override Action checkMutation(Application app, Index pos) {
			BlurApp bApp = (BlurApp)app;
			BlurAction action = new BlurAction();
			float voxelSize = LocalMutator.calculateVoxelSize(app, pos);
			Vector3 diff = LocalMutator.calculateDiff(bApp.position, pos, voxelSize);

			action.disSqr = diff.sqrMagnitude;
			float maxRadius = bApp.radius + voxelSize;
			float maxRadSqr = maxRadius * maxRadius;
			if (action.disSqr > maxRadSqr)
				return action;
			action.doTraverse = true;
			action.modify = true;
			return action;
		}

		protected override Voxel mutate(Application app, Index pos, Action action, Voxel original) {
			BlurApp bApp = (BlurApp)app;
			BlurAction bAction = (BlurAction)action;
			float dis = Mathf.Sqrt(bAction.disSqr);
			float actualStrength = strength * (1 - (dis / bApp.radius));
			if (actualStrength <= 0)
				return original;
			byte newOpacity = calculateOpacity(bApp.original, pos.x - app.min.x, pos.y - app.min.y, pos.z - app.min.z, actualStrength);
			return new Voxel(original.averageMaterialType(), newOpacity);
		}

		protected class BlurApp: LocalMutator.LocalApplication {
			public float minRadius, maxRadius;
			public Voxel[,,] original;
			public float radius;

			public void setOriginal(OcTree target) {
				original = new ArrayReader(min, max.getNeighbor(1, 1, 1)).read(target);
			}
		}

		protected class BlurAction: Action {
			public float disSqr;

			public BlurAction() : base(false, false) { }
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
	}
}