using UnityEngine;
using System.Collections;
using System;

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

		public override Application setup(OcTree target) {
			Vector3 halfDimension = worldDimensions / target.voxelSize /2f;
			Vector3 center = target.transform.InverseTransformPoint(worldPosition) / target.voxelSize;
			Vector3 exactMin = center - halfDimension;
			Vector3 exactMax = center + halfDimension;

			CubeApp app = new CubeApp();
			app.tree = target;
			app.halfDimension = halfDimension;
			app.min = new Index(target.maxDepth, (uint)exactMin.x, (uint)exactMin.y, (uint)exactMin.z);
			app.max = new Index(target.maxDepth, (uint)exactMax.x, (uint)exactMax.y, (uint)exactMax.z);
			app.position = center;
			return app;
		}

		public override LocalAction checkMutation(LocalApplication app, Index p, Vector3 diff, float voxelSize) {
			CubeApp cApp = (CubeApp)app;
			CubeAction action = new CubeAction();
			if (p.depth >= app.tree.maxDepth)
				voxelSize *= 0.5f;

			action.percentInside = 1;
			bool outside = false;
			bool inside = true;

			action.percentInside *= 1 - (2 - percentOverlapping(diff.x, cApp.halfDimension.x, voxelSize, ref outside, ref inside)
				- percentOverlapping(-diff.x, cApp.halfDimension.x, voxelSize, ref outside, ref inside));
			if (outside) return action;
			action.percentInside *= 1 - (2 - percentOverlapping(diff.y, cApp.halfDimension.y, voxelSize, ref outside, ref inside)
				- percentOverlapping(-diff.y, cApp.halfDimension.y, voxelSize, ref outside, ref inside));
			if (outside) return action;
			action.percentInside *= 1 - (2 - percentOverlapping(diff.z, cApp.halfDimension.z, voxelSize, ref outside, ref inside)
				- percentOverlapping(-diff.z, cApp.halfDimension.z, voxelSize, ref outside, ref inside));
			if (outside) return action;

			action.modify = true;
			if (!overwriteShape || !inside)
				action.doTraverse = true;
			return action;
		}

		public override Voxel mutate(LocalApplication app, Index p, LocalAction action, Voxel original) {
			CubeAction cAction = (CubeAction)action;
			byte newOpacity = (byte)((original.averageOpacity() * (1 - cAction.percentInside) + value.averageOpacity() * (cAction.percentInside)));
			byte newSubstance = original.averageMaterialType();
			if (overwriteSubstance && cAction.percentInside > 0.5)
				newSubstance = value.averageMaterialType();
			if (!overwriteShape)
				newOpacity = original.averageOpacity();
			return new Voxel(newSubstance, newOpacity);
		}

		protected double percentOverlapping(double lower, double upper, double halfVoxelSize, ref bool outside, ref bool inside) {
            if (upper > lower + halfVoxelSize) {
				outside |= false;
				inside &= true;
				return 1;
			} else if (upper < lower - halfVoxelSize) {
				outside = true;
				inside = false;
				return 0;
			}
			outside |= false;
			inside = false;
			return (upper - lower + halfVoxelSize) /halfVoxelSize /2.0;
		}

		protected class CubeApp : LocalApplication {
			public Vector3 halfDimension;
		}

		protected class CubeAction: LocalAction {
			public double percentInside;
			public CubeAction() : base(false, false) { }
		}

	}
}
