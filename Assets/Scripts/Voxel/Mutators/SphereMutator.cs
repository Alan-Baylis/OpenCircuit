using UnityEngine;

namespace Vox {

	public class SphereMutator : LocalMutator {

		public Voxel value;
		public bool overwriteSubstance = true;
		public bool overwriteShape = true;
		public float worldRadius;
		public Vector3 worldPosition;

		public SphereMutator(Vector3 worldPosition, float worldRadius, Voxel value) {
			this.value = value;
			this.worldPosition = worldPosition;
			this.worldRadius = worldRadius;
		}

		public override Application setup(OcTree target) {
			float radius = worldRadius / target.voxelSize;
			Vector3 radiusCube = new Vector3(radius, radius, radius);
			Vector3 center = target.globalToVoxelPosition(worldPosition);
			Vector3 exactMin = center - radiusCube;
			Vector3 exactMax = center + radiusCube;
			SphereApp app = new SphereApp();
			app.tree = target;
			app.min = new Index(target.maxDepth, (uint)exactMin.x, (uint)exactMin.y, (uint)exactMin.z);
			app.max = new Index(target.maxDepth, (uint)exactMax.x, (uint)exactMax.y, (uint)exactMax.z);
			app.position = center;
			app.radius = radius;
			return app;
		}

		public override LocalAction checkMutation(LocalApplication app, Index p, Vector3 diff, float voxelSize, bool canTraverse) {
			SphereApp sApp = (SphereApp)app;
			SphereAction action = new SphereAction();
			action.disSqr = diff.sqrMagnitude;
			action.maxRadius = sApp.radius + voxelSize;
			float maxRadSqr = action.maxRadius * action.maxRadius;
			if (action.disSqr > maxRadSqr)
				return action;
			action.modify = true;
			action.minRadius = Mathf.Max(0, sApp.radius - voxelSize);
			float minRadSqr = action.minRadius * action.minRadius;
			if (!overwriteShape || action.disSqr >= minRadSqr)
				action.doTraverse = true;
			return action;
		}

		public override Voxel mutate(LocalApplication app, Index p, LocalAction action, Voxel original) {
			SphereApp sApp = (SphereApp)app;
			SphereAction sAction = (SphereAction)action;

			float dis = Mathf.Sqrt(sAction.disSqr);
			float percentInside = Mathf.Min((sAction.maxRadius -dis) /(sAction.maxRadius -sAction.minRadius), 1);
			byte newOpacity = (byte)(original.averageOpacity() * (1 -percentInside) + value.averageOpacity() * percentInside);
			byte newSubstance = original.averageMaterialType();
			if (overwriteSubstance && (dis < sApp.radius || percentInside * byte.MaxValue > original.averageOpacity() *0.5f))
				newSubstance = value.averageMaterialType();
			if (!overwriteShape)
				newOpacity = original.averageOpacity();
			return new Voxel(newSubstance, newOpacity);
		}

		protected class SphereApp: LocalApplication {
			public float radius;
		}

		protected class SphereAction: LocalAction {
			public float disSqr, minRadius, maxRadius;

			public SphereAction(): base(false, false) {}
		}

	}
}
