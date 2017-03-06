using UnityEngine;

namespace Vox {

	public class LineMutator : LocalMutator {

		public LocalMutator child;
		public Vector3[] globalPoints;

		public LineMutator(Vector3[] globalPoints, LocalMutator child) {
			if (globalPoints.Length < 2)
				throw new System.ArgumentException("Must have at least two points specified.", "globalPoints");
			this.globalPoints = globalPoints;
			this.child = child;
		}

		public override App init(OcTree tree) {
			LineApp app = new LineApp(tree);
			app.position = tree.globalToVoxelPosition(globalPoints[0]);
			app.points = new Vector3[globalPoints.Length];
			for (int i = 0; i < globalPoints.Length; ++i)
				app.points[i] = tree.globalToVoxelPosition(globalPoints[i]) -app.position;
			app.childApp = child.init(tree);
			// TODO: set min, max and updateMesh.
			return app;
		}

		public override Act checkMutation(App application, Index p, Vector3 diff, float voxelSize, bool canTraverse) {
			LineApp app = (LineApp) application;
			Vector3 cp = closestPointToPath(app.points, diff);
			Vector3 virtualDiff = diff - cp;
			Act act = child.checkMutation(app.childApp, p, virtualDiff, voxelSize, canTraverse);
			((LocalActCache)act.cache).diff = virtualDiff;
			return act;
		}

		public override Voxel mutate(App application, Index p, Act act, Voxel original) {
			LineApp app = (LineApp) application;
			return child.mutate(app.childApp, p, act, original);
		}

		protected Vector3 closestPointToPath(Vector3[] points, Vector3 point) {
			float leastSqrDistance = float.PositiveInfinity;
			Vector3 closestPoint = Vector3.zero;
			for (int i = 0; i < points.Length - 1; ++i) {
				Vector3 newClosestPoint = closestPointToLine(points[i], points[i + 1], point);
				float sqrDistance = (point - newClosestPoint).sqrMagnitude;
				if (sqrDistance < leastSqrDistance) {
					leastSqrDistance = sqrDistance;
					closestPoint = newClosestPoint;
				}
			}
			return closestPoint;
		}

		protected Vector3 closestPointToLine(Vector3 start, Vector3 end, Vector3 point) {
			Vector3 line = end -start;
			float percent = Vector3.Dot(point - start, line) / line.sqrMagnitude;
			percent = Mathf.Clamp01(percent);
			Vector3 closest = start +line *percent;
			return closest;
		}

		public class LineApp : LocalApp {
			public Vector3[] points;
			public App childApp;

			public LineApp(OcTree tree) : base(tree) { }
		}
	}
}
