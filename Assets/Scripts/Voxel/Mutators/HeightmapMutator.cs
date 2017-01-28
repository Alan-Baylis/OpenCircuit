using UnityEngine;
using System;
using System.Deployment.Internal;

namespace Vox {

	public class HeightmapMutator : LocalMutator {

		public double[,] map;
		public byte[,] substanceMap;
		public Vector3 min, max;
		public bool overwriteSubstance = true;
		public bool overwriteShape = true;

		public Vector3 worldPosition;
		private Vector3 worldDimensions;

		public HeightmapMutator(Vector3 worldPosition, Vector3 worldDimensions, double[,] heightmap, byte[,] substanceMap) {
			this.worldPosition = worldPosition;
			this.worldDimensions = worldDimensions;
			this.map = heightmap;
			this.substanceMap = substanceMap;
		}

		public HeightmapMutator(Vector3 worldPosition, Vector3 worldDimensions, double[,] heightmap, byte substance)
			: this(worldPosition, worldDimensions, heightmap, new byte[1, 1] { { substance } }) { }

		public override Application setup(OcTree target) {
			Vector3 halfDimension = worldDimensions / target.voxelSize /2f;
			Vector3 center = target.transform.InverseTransformPoint(worldPosition) / target.voxelSize;
			Vector3 exactMin = center - halfDimension;
			Vector3 exactMax = center + halfDimension;

			HeightApp app = new HeightApp();
			app.tree = target;
			app.halfDimension = halfDimension;
			app.min = new Index(target.maxDepth, (uint)exactMin.x, (uint)exactMin.y, (uint)exactMin.z);
			app.max = new Index(target.maxDepth, (uint)exactMax.x, (uint)exactMax.y, (uint)exactMax.z);
			app.position = center;
			return app;
		}

		public override LocalAction checkMutation(LocalApplication app, Index p, Vector3 diff, float voxelSize, bool canTraverse) {
			HeightApp hApp = (HeightApp)app;
			HeightAction action = new HeightAction();
			if (p.depth >= app.tree.maxDepth)
				voxelSize *= 0.5f;

			action.percentInside = 1;
			bool outside = false;
			bool inside = true;

			action.percentInside *= -1 + percentOverlapping(diff.x, hApp.halfDimension.x, voxelSize, ref outside, ref inside)
				+ percentOverlapping(-diff.x, hApp.halfDimension.x, voxelSize, ref outside, ref inside);
			if (outside) return action;

			action.percentInside *= -1 + percentOverlapping(diff.z, hApp.halfDimension.z, voxelSize, ref outside, ref inside)
				+ percentOverlapping(-diff.z, hApp.halfDimension.z, voxelSize, ref outside, ref inside);
			if (outside) return action;

			action.percentInside *= -1 + percentInMap(diff, hApp.halfDimension, voxelSize, ref outside, ref inside, canTraverse)
				+ percentOverlapping(-diff.y, hApp.halfDimension.y, voxelSize, ref outside, ref inside);
			if (outside) return action;

			action.modify = true;
			if (!overwriteShape || !inside)
				action.doTraverse = true;
			return action;
		}

		public override Voxel mutate(LocalApplication app, Index p, LocalAction action, Voxel original) {
			HeightAction hAction = (HeightAction)action;
            byte newOpacity = (byte)(original.averageOpacity() * (1 - hAction.percentInside) + Voxel.full.averageOpacity() *hAction.percentInside);
			byte newSubstance = original.averageMaterialType();
			if (overwriteSubstance && hAction.percentInside > 0.5)
				newSubstance = hAction.averageSubstance;
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

		protected double percentInMap(Vector3 diff, Vector3 halfDimmension, double halfVoxelSize, ref bool outside, ref bool inside, bool needPercent=true) {
			double columnAreaPercent = 1 / (halfVoxelSize /halfDimmension.x *map.GetLength(0)) / (halfVoxelSize /halfDimmension.z *map.GetLength(1));
			double xMin = Math.Max(0, (diff.x - halfVoxelSize + halfDimmension.x) / 2 / halfDimmension.x) * map.GetLength(0);
			double xMax = Math.Min(1, (diff.x + halfVoxelSize + halfDimmension.x) / 2 / halfDimmension.x) * map.GetLength(0);
			double zMin = Math.Max(0, (diff.z - halfVoxelSize + halfDimmension.z) / 2 / halfDimmension.z) * map.GetLength(1);
			double zMax = Math.Min(1, (diff.z + halfVoxelSize + halfDimmension.z) / 2 / halfDimmension.z) * map.GetLength(1);
			double percent = 0;
			double lastHeightPercent = -1;
			bool intersecting = false;
			int hits = 0;
			for (uint x = (uint)xMin; x < xMax; ++x) {
				for (uint z = (uint)zMin; z < zMax; ++z) {
					double edgePercent = 1;
					if (x < xMin) edgePercent *= 1 - (xMin - x);
					if (x > xMax -1) edgePercent *= 1 - (x - xMax + 1);
					if (z < zMin) edgePercent *= 1 - (zMin - z);
					if (z > zMax -1) edgePercent *= 1 - (z - zMax + 1);
					++hits;
					double heightPercent = Math.Max(0, Math.Min(1, (map[x, z] * halfDimmension.y * 2 -halfDimmension.y - diff.y +halfVoxelSize) / halfVoxelSize / 2));
					if (heightPercent > 0)
						intersecting = true;
					if (heightPercent < 1)
						inside = false;
					if (lastHeightPercent != -1 && lastHeightPercent != heightPercent && !inside && !needPercent)
						return 0.5f;
					lastHeightPercent = heightPercent;
					percent += edgePercent *heightPercent;
				}
			}
			if (!intersecting) {
				outside = true;
				inside = false;
			}

			return percent *columnAreaPercent;
		}


		protected class HeightApp : LocalApplication {
			//public byte depth;
			public Vector3 halfDimension;
		}

		protected class HeightAction : LocalAction {
			public double percentInside;
			public byte averageSubstance;
			public HeightAction() : base(false, false) { }
		}

	}
}
