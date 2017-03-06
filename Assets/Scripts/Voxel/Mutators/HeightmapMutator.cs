using UnityEngine;
using System;


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

		public override App init(OcTree tree) {
			HeightApp app = new HeightApp(tree);
			Vector3 halfDimension = worldDimensions / app.tree.voxelSize /2f;
			Vector3 center = app.tree.transform.InverseTransformPoint(worldPosition) / app.tree.voxelSize;
			Vector3 exactMin = center - halfDimension;
			Vector3 exactMax = center + halfDimension;

			app.min = new Index(app.tree.maxDepth, (uint) exactMin.x, (uint) exactMin.y, (uint) exactMin.z);
			app.max = new Index(app.tree.maxDepth, (uint) exactMax.x, (uint) exactMax.y, (uint) exactMax.z);
			app.halfDimension = halfDimension;
			app.position = center;
			return app;
		}

		public override Act checkMutation(App application, Index p, Vector3 diff, float voxelSize, bool canTraverse) {
			HeightApp app = (HeightApp) application;
			ActCache actCache = new ActCache();
			Act act = new Act(actCache);
			if (p.depth >= app.tree.maxDepth)
				voxelSize *= 0.5f;

			actCache.percentInside = 1;
			bool outside = false;
			bool inside = true;

			actCache.percentInside *= -1 + percentOverlapping(diff.x, app.halfDimension.x, voxelSize, ref outside, ref inside)
				+ percentOverlapping(-diff.x, app.halfDimension.x, voxelSize, ref outside, ref inside);
			if (outside) return act;

			actCache.percentInside *= -1 + percentOverlapping(diff.z, app.halfDimension.z, voxelSize, ref outside, ref inside)
				+ percentOverlapping(-diff.z, app.halfDimension.z, voxelSize, ref outside, ref inside);
			if (outside) return act;

			actCache.percentInside *= -1 + percentInMap(diff, app.halfDimension, voxelSize, ref outside, ref inside, canTraverse)
				+ percentOverlapping(-diff.y, app.halfDimension.y, voxelSize, ref outside, ref inside);
			if (outside) return act;

			act.modify = true;
			if (!overwriteShape || !inside)
				act.doTraverse = true;
			return act;
		}

		public override Voxel mutate(App application, Index p, Act act, Voxel original) {
			ActCache actCache = (ActCache) act.cache;
            byte newOpacity = (byte)(original.averageOpacity() * (1 - actCache.percentInside) + Voxel.full.averageOpacity() *actCache.percentInside);
			byte newSubstance = original.averageMaterialType();
			if (overwriteSubstance && actCache.percentInside > 0.5)
				newSubstance = actCache.averageSubstance;
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


		public class HeightApp : LocalApp {
			public Vector3 halfDimension;

			public HeightApp(OcTree tree) : base(tree) { }
		}

		public class ActCache : LocalActCache {
			public double percentInside;
			public byte averageSubstance;
		}

	}
}
