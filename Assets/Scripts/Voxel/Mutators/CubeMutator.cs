using UnityEngine;

namespace Vox {

	public class CubeMutator : LocalMutator {

		public Voxel value;
		public Vector3 min, max;
		public bool overwriteSubstance = true;
		public bool overwriteShape = true;

		public Vector3 worldPosition;
		private Vector3 worldDimensions;

		public CubeMutator(Vector3 worldPosition, Vector3 worldDimensions, VoxelHolder value) {
			this.worldPosition = worldPosition;
			this.worldDimensions = worldDimensions;
			this.value = value.toVoxel();
		}

		public override App init(OcTree tree) {
			CubeApp app = new CubeApp(tree);
			Vector3 halfDimension = worldDimensions / tree.voxelSize /2f;
			Vector3 center = tree.transform.InverseTransformPoint(worldPosition) / tree.voxelSize;
			Vector3 exactMin = center - halfDimension;
			Vector3 exactMax = center + halfDimension;

			app.min = new Index(tree.maxDepth, (uint)exactMin.x, (uint)exactMin.y, (uint)exactMin.z);
			app.max = new Index(tree.maxDepth, (uint)exactMax.x, (uint)exactMax.y, (uint)exactMax.z);
			app.halfDimension = halfDimension;
			app.position = center;
			return app;
		}

		public override Act checkMutation(App application, Index p, Vector3 diff, float voxelSize, bool canTraverse) {
			CubeApp app = (CubeApp) application;
			ActCache actCache = new ActCache();
			Act act = new Act(actCache);
			if (p.depth >= app.tree.maxDepth)
				voxelSize *= 0.5f;

			actCache.percentInside = 1;
			bool outside = false;
			bool inside = true;

			actCache.percentInside *= 1 - (2 - percentOverlapping(diff.x, app.halfDimension.x, voxelSize, ref outside, ref inside)
				- percentOverlapping(-diff.x, app.halfDimension.x, voxelSize, ref outside, ref inside));
			if (outside) return act;
			actCache.percentInside *= 1 - (2 - percentOverlapping(diff.y, app.halfDimension.y, voxelSize, ref outside, ref inside)
				- percentOverlapping(-diff.y, app.halfDimension.y, voxelSize, ref outside, ref inside));
			if (outside) return act;
			actCache.percentInside *= 1 - (2 - percentOverlapping(diff.z, app.halfDimension.z, voxelSize, ref outside, ref inside)
				- percentOverlapping(-diff.z, app.halfDimension.z, voxelSize, ref outside, ref inside));
			if (outside) return act;

			act.modify = true;
			if (!overwriteShape || !inside)
				act.doTraverse = true;
			return act;
		}

		public override Voxel mutate(App app, Index p, Act act, Voxel original) {
			ActCache actCache = (ActCache) act.cache;
			byte newOpacity = (byte)(original.averageOpacity() * (1 - actCache.percentInside) +
			                         value.averageOpacity() * actCache.percentInside);
			byte newSubstance = original.averageMaterialType();
			if (overwriteSubstance && actCache.percentInside > 0.5)
				newSubstance = value.averageMaterialType();
			if (!overwriteShape)
				newOpacity = original.averageOpacity();
			return new Voxel(newSubstance, newOpacity);
		}

		protected double percentOverlapping(double lower, double upper, double halfVoxelSize, ref bool outside, ref bool inside) {
            if (upper > lower + halfVoxelSize) {
				return 1;
			} else if (upper < lower - halfVoxelSize) {
				outside = true;
				inside = false;
				return 0;
			}
			inside = false;
			return (upper - lower + halfVoxelSize) /halfVoxelSize /2.0;
		}

		public class CubeApp : LocalApp {
			public Vector3 halfDimension;

			public CubeApp(OcTree tree) : base(tree) { }
		}

		public class ActCache: LocalActCache {
			public double percentInside;
		}

	}
}
