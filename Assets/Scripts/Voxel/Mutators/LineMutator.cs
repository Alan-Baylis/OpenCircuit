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

		public override Application setup(OcTree target) {

			LineApplication app = new LineApplication();
			app.tree = target;
			app.position = target.globalToVoxelPosition(globalPoints[0]);
			app.points = new Vector3[globalPoints.Length];
			for (int i = 0; i < globalPoints.Length; ++i)
				app.points[i] = target.globalToVoxelPosition(globalPoints[i]) -app.position;
            app.childApp = (LocalApplication) child.setup(target);
			// TODO: set min, max and updateMesh.

			return app;
		}

		protected override Action checkMutation(Application app, Index p) {
			LocalApplication lApp = (LocalApplication)app;
			float voxelSize = calculateVoxelSize(app, p);
			Vector3 diff = calculateDiff(lApp.position, p, voxelSize);
			LocalAction action = checkMutation(lApp, p, diff, voxelSize);
			action.voxelSize = voxelSize;
			return action;
		}

		public override LocalAction checkMutation(LocalApplication app, Index p, Vector3 diff, float voxelSize) {
			LineApplication lApp = (LineApplication) app;
			Vector3 cp = closestPointToPath(lApp.points, diff);
			Vector3 virtualDiff = diff - cp;
			LocalAction action = child.checkMutation(((LineApplication)app).childApp, p, virtualDiff, voxelSize);
			action.diff = virtualDiff;
			return action;
		}

		public override Voxel mutate(LocalApplication app, Index p, LocalAction action, Voxel original) {
			return child.mutate(((LineApplication)app).childApp, p, action, original);
		}

		protected Vector3 closestPointToPath(Vector3[] points, Vector3 point) {
			float leastSqrDistance = float.PositiveInfinity;
			Vector3 closestPoint = Vector3.zero;
			for (int i = 0; i < points.Length - 1; ++i) {
				Vector3 newClosestPoint = this.closestPointToLine(points[i], points[i + 1], point);
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

		protected class LineApplication : LocalApplication {
			public Vector3[] points;
			public LocalApplication childApp;
		}

	}
}
