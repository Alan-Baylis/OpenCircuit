using UnityEngine;
using System.Collections;
using System.IO;
using System;

namespace Vox {

	[System.Serializable]
	public class Voxel : VoxelHolder {

		public static readonly Voxel empty = new Voxel(0, 0);
		public static readonly Voxel full = new Voxel(0, byte.MaxValue);

		public readonly byte opacity;
		public readonly byte matType;

		public Voxel(byte materialType, byte opacity) {
			this.matType = materialType;
			this.opacity = opacity;
		}

		public Voxel(Voxel other) {
			this.matType = other.matType;
			this.opacity = other.opacity;
		}

		public Voxel(BinaryReader reader) {
			this.matType = reader.ReadByte();
			this.opacity = reader.ReadByte();
		}

		public override void serialize(BinaryWriter writer) {
			writer.Write(VoxelHolder.VOXEL_SERIAL_ID);
			writer.Write(matType);
			writer.Write(opacity);
		}

		public override byte detail() {
			return 0;
		}

		public override byte averageOpacity() {
			return opacity;
		}

		public override byte averageMaterialType() {
			return matType;
		}

		public override VoxelHolder get(byte detailLevel, uint x, uint y, uint z) {
			return this;
		}
		
		public override VoxelHolder get(Index i) {
			return this;
		}

		public override void putInArray(ref Voxel[,,] array, Index position, uint xMin, uint yMin, uint zMin, uint xMax, uint yMax, uint zMax) {
			uint size = 1u << (VoxelBlock.CHILD_COUNT_POWER *position.depth);
			uint xStart = (uint)Mathf.Max(position.x, xMin) -xMin;
			uint xEnd = (uint)Mathf.Min(position.x +size, xMax) -xMin;
			uint yStart = (uint)Mathf.Max(position.y, yMin) -yMin;
			uint yEnd = (uint)Mathf.Min(position.y +size, yMax) -yMin;
			uint zStart = (uint)Mathf.Max(position.z, zMin) -zMin;
			uint zEnd = (uint)Mathf.Min(position.z +size, zMax) -zMin;
			for(uint xi=xStart; xi<xEnd; ++xi) {
				for(uint yi=yStart; yi<yEnd; ++yi) {
					for(uint zi=zStart; zi<zEnd; ++zi) {
						array[xi, yi, zi] = this;
					}
				}
			}
		}

		public override int canSimplify(out Voxel simplification) {
			simplification = this;
			return 0;
		}

		public override int cleanArtifacts(out Voxel simplified, VoxelHolder head, byte level, byte maxLevel, int x, int y, int z) {
			simplified = null;
			if (level < maxLevel)
				return 0;
			Voxel[,,] array = new Voxel[3, 3, 3];
			head.putInArray(ref array, new Index(level), (uint)x -1, (uint)y -1, (uint)z -1, (uint)x +2, (uint)y +2, (uint)z +2);
			bool solid = isSolid();
			foreach(Voxel vox in array) {
				if (vox != null && vox.isSolid() != solid)
					return 0;
			}
			simplified = solid? full : empty;
			return 1;
		}

		public static VoxelHolder setSphere(VoxelHolder original, int x, int y, int z, Vector3 min, Vector3 max, VoxelHolder val) {
			Vector3 center = (min + max) / 2;
			float radius = center.x - min.x;
			float minDis = (radius - 1);
			float maxDis = (radius + 1);
			float dis = (center - new Vector3(x, y, z)).magnitude;
			if (dis > maxDis)
				return original;
			if (dis < minDis)
				return val;
			byte newOpacity = (byte)((original.averageOpacity() * (dis - minDis) + val.averageOpacity() * (maxDis - dis)) /2);
			if ((dis - minDis) > 0.5f)
				return new Voxel(val.averageMaterialType(), newOpacity);
			return new Voxel(original.averageMaterialType(), newOpacity);
		}

		public override Voxel toVoxel() {
			return this;
		}

	}

}