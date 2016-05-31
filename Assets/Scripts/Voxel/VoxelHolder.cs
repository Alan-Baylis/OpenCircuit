using UnityEngine;
using System.Collections;
using System.IO;

namespace Vox {
	
//	[System.Serializable]
	public abstract class VoxelHolder {

		public const byte VOXEL_SERIAL_ID = 0;
		public const byte VOXEL_BLOCK_SERIAL_ID = 1;

		public static int blockCount = 0;

		// returns how many more layers there are beneath this one
		public abstract byte detail();

		public abstract byte averageOpacity();

		public abstract byte averageMaterialType();

		// relative to me.  detail level 1 means go one further, no matter what. index also relative to me
		public abstract VoxelHolder get(byte detailLevel, uint x, uint y, uint z);

		public abstract VoxelHolder get(Index i);

		public abstract Voxel toVoxel();

		public abstract void serialize(BinaryWriter writer);

		public abstract void putInArray(ref Voxel[,,] array, Index position, uint xMin, uint yMin, uint zMin, uint xMax, uint yMax, uint zMax);

		public abstract int canSimplify(out Voxel simplification);

		public abstract int cleanArtifacts(out Voxel simplified, VoxelHolder head, byte level, byte maxLevel, int x, int y, int z);

		public bool isSolid() {
			return averageOpacity() > byte.MaxValue /2;
		}

		public static VoxelHolder deserialize(BinaryReader reader) {
			byte type = reader.ReadByte();
			switch(type) {
			case VOXEL_BLOCK_SERIAL_ID:
				return new VoxelBlock(reader);
			case VOXEL_SERIAL_ID:
				return new Voxel(reader);
			}
			return null;
		}

		public static bool operator ==(VoxelHolder v1, VoxelHolder v2) {
			if (System.Object.ReferenceEquals(v1, v2))
				return true;
			if (((object)v1 == null) || ((object)v2 == null))
				return false;
			return (v1.averageMaterialType() == v2.averageMaterialType() && v1.averageOpacity() == v2.averageOpacity());
		}

		public static bool operator !=(VoxelHolder v1, VoxelHolder v2) {
			return !(v1 == v2);
		}
		
		public override bool Equals(object ob) {
			if (ob == null || GetType () != ob.GetType())
				return false;
			return this == (VoxelHolder)ob;
		}

		// NOTE: could be very expensive depending on the specific implementation
		public override int GetHashCode() {
			int hashCode = 13;
			hashCode = hashCode *7 +averageMaterialType();
			hashCode = hashCode *7 +averageOpacity();
			return hashCode;
		}

	}

}