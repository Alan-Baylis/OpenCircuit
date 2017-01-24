using UnityEngine;
using System.Collections;

namespace Vox {
	public abstract class LocalMutator: Mutator {

		protected override Action checkMutation(Application app, Index p) {
			LocalApplication lApp = (LocalApplication)app;
			float voxelSize = calculateVoxelSize(app, p);
			Vector3 diff = calculateDiff(lApp.position, p, voxelSize);
			LocalAction action = checkMutation(lApp, p, diff, voxelSize);
			action.voxelSize = voxelSize;
			action.diff = diff;
			return action;
		}

		protected override Voxel mutate(Application app, Index p, Action action, Voxel original) {
			return mutate((LocalApplication)app, p, (LocalAction)action, original);
		}

		public abstract LocalAction checkMutation(LocalApplication app, Index p, Vector3 diff, float voxelSize);

		public abstract Voxel mutate(LocalApplication app, Index p, LocalAction action, Voxel original);

		public static float calculateVoxelSize(Application app, Index p) {
			return 1 << (app.tree.maxDepth - p.depth);
		}

		public static Vector3 calculateDiff(Vector3 position, Index p, float voxelSize) {
			Vector3 diff = new Vector3(p.x + 0.5f, p.y + 0.5f, p.z + 0.5f) * voxelSize -position;
			return diff;
		}

		public class LocalApplication: Application {
			public Vector3 position;
		}

		public class LocalAction: Action {

			public float voxelSize;
			public Vector3 diff;

			public LocalAction(bool doTraverse, bool modified): base(doTraverse, modified) {}
		}

	}
}