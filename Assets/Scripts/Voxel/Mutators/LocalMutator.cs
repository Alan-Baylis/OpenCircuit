using UnityEngine;

namespace Vox {
	public abstract class LocalMutator: Mutator {

		public override Act checkMutation(App application, Index p) {
			LocalApp app = (LocalApp)application;
			float voxelSize = calculateVoxelSize(app, p);
			Vector3 diff = calculateDiff(app.position, p, voxelSize);
			Act action = checkMutation(app, p, diff, voxelSize, canTraverse(p, app.tree));
			if (!action.modify)
				return action;
			if (action.cache == null) {
				action.cache = new LocalActCache();
			}
			LocalActCache actCache = action.cache as LocalActCache;
			if (actCache != null) {
				actCache.voxelSize = voxelSize;
				actCache.diff = diff;
			}
			return action;
		}

		public abstract Act checkMutation(App app, Index p, Vector3 diff, float voxelSize, bool canTraverse);

		public class LocalApp : App {
			public Vector3 position;

			public LocalApp(OcTree tree) : base(tree) {}
		}

		public class LocalActCache {
			public float voxelSize;
			public Vector3 diff;
		}

	}
}