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

		public override App init(OcTree tree) {
			SphereApp app = new SphereApp(tree);
			float radius = worldRadius / tree.voxelSize;
			Vector3 radiusCube = new Vector3(radius, radius, radius);
			Vector3 center = tree.globalToVoxelPosition(worldPosition);
			Vector3 exactMin = center - radiusCube;
			Vector3 exactMax = center + radiusCube;

			app.min = new Index(tree.maxDepth, (uint) exactMin.x, (uint) exactMin.y, (uint) exactMin.z);
			app.max = new Index(tree.maxDepth, (uint) exactMax.x, (uint) exactMax.y, (uint) exactMax.z);
			app.position = center;
			app.radius = radius;
			return app;
		}

		public override Act checkMutation(App application, Index p, Vector3 diff, float voxelSize, bool canTraverse) {
			SphereApp app = (SphereApp) application;
			float maxVoxelWidth = voxelSize *0.8f;
			float maxRadius = app.radius + maxVoxelWidth;
			float dis = diff.magnitude;
			float minRadius = Mathf.Max(0, app.radius - maxVoxelWidth);
			float percentInside = Mathf.Min((maxRadius -dis) /(maxRadius -minRadius), 1);

			// check if completely outside
			if (percentInside <= 0.01f)//disSqr > maxRadius * maxRadius)
				return new Act(false, false);

			// setup cache in case needed later
			ActCache actCache = new ActCache {
				dis = dis,
				percentInside = percentInside
			};
			Act act = new Act(actCache, true, !overwriteShape || percentInside < 0.99f);
			return act;
		}

		public override Voxel mutate(App application, Index p, Act act, Voxel original) {
			SphereApp app = (SphereApp) application;
			ActCache actCache = (ActCache) act.cache;

//			float dis = Mathf.Sqrt(actCache.disSqr);
//			float percentInside = Mathf.Min((actCache.maxRadius -dis) /(actCache.maxRadius -actCache.minRadius), 1);
			byte newOpacity = (byte)(original.averageOpacity() * (1 -actCache.percentInside) + value.averageOpacity() * actCache.percentInside);
			byte newSubstance = original.averageMaterialType();
			if (overwriteSubstance && (actCache.dis < app.radius || actCache.percentInside * byte.MaxValue > original.averageOpacity() *0.5f))
				newSubstance = value.averageMaterialType();
			if (!overwriteShape)
				newOpacity = original.averageOpacity();
			return new Voxel(newSubstance, newOpacity);
		}

		public class SphereApp: LocalApp {
			public float radius;

			public SphereApp(OcTree tree) : base(tree) { }
		}

		public class ActCache: LocalActCache {
			public float dis, percentInside;//, minRadius, maxRadius;
		}

	}
}
