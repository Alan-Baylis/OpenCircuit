
using System;

namespace Vox {
	
	[Serializable]
	public struct Index {

		public static readonly Index ZERO = new Index(0);

		public readonly byte depth;
		public readonly uint x, y, z;

		public uint xLocal {get {
			return x % VoxelBlock.CHILD_DIMENSION;
		}}
		public uint yLocal {get {
			return y % VoxelBlock.CHILD_DIMENSION;
		}}
		public uint zLocal {get {
			return z % VoxelBlock.CHILD_DIMENSION;
		}}

		public Index bounded {get {
			return limit((uint) 1L << depth);
		}}

		public Index(byte depth) : this(depth, 0) {}

		public Index(byte depth, uint all) : this(depth, all, all, all) {}

		public Index(byte depth, uint x, uint y, uint z) {
			this.depth = depth;
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Index getChild() {
			return new Index((byte)(depth +1), x*2, y*2, z*2);
		}

		/// <summary>
		/// Gets one of the eight children of an index using a bit encoded xyz value.
		/// </summary>
		/// <param name="i">the bit encoded xyz local position of the child ranging from 0-7</param>
		/// <returns></returns>
		public Index getChild(byte i) {
			return new Index((byte)(depth +1),
				(uint)(x *2 +((i &4) >> 2)),
				(uint)(y *2 +((i &2) >> 1)),
				(uint)(z *2 +(i &1)));
		}

		/// <summary>
		/// Gets one of seven neighbors of an index using a bit encoded xyz value.
		/// </summary>
		/// <param name="i">the bit encoded xyz offset to the neighbor, 0 referring to itself and 7
		/// referring to the last neighbor touching it one corner</param>
		/// <returns></returns>
		public Index getNeighbor(byte i) {
			return new Index(depth,
				(uint)(x +((i &4) >> 2)),
				(uint)(y +((i &2) >> 1)),
				(uint)(z +(i &1)));
		}

		public Index getNeighbor(int xOffset, int yOffset, int zOffset) {
			return new Index(depth,
				(uint)(x +xOffset),
				(uint)(y +yOffset),
				(uint)(z +zOffset));
		}

		public Index getParent() {
			return new Index((byte)(depth -1), x / 2, y / 2, z / 2);
		}

		public Index getLevel(byte lDepth) {
			int diff = lDepth - depth;
			if (diff >= 0)
				return new Index(lDepth, x << diff, y << diff, z << diff);
			diff = -diff;
			return new Index(lDepth, x >> diff, y >> diff, z >> diff);
		}

		public override bool Equals(object ob) {
			if (ob == null || GetType() != ob.GetType())
				return false;
			return this == (Index)ob;
		}

		public static bool operator ==(Index v1, Index v2) {
			return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z && v1.depth == v2.depth;
		}
		
		public static bool operator !=(Index v1, Index v2) {
			return !(v1 == v2);
		}

		public static Index operator -(Index lhs, Index rhs) {
			normalizeDepth(ref lhs, ref rhs);
			return new Index(rhs.depth, lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
		}

		public static void normalizeDepth(ref Index one, ref Index two) {
			if (one.depth < two.depth) {
				one = one.getLevel(two.depth);
			} else if (one.depth > two.depth) {
				two = two.getLevel(one.depth);
			}
		}

		public static Index maxMerge(params Index[] indices) {
			Index max = ZERO;
			foreach(Index index in indices) {
				Index i = index;
				normalizeDepth(ref max, ref i);
				max = new Index(max.depth,
					Math.Max(i.x, max.x),
					Math.Max(i.y, max.y),
					Math.Max(i.z, max.z));
			}
			return max;
		}

		public static Index minMerge(params Index[] indices) {
			Index min = new Index(0, uint.MaxValue);
			foreach(Index index in indices) {
				Index i = index;
				normalizeDepth(ref min, ref i);
				min = new Index(min.depth,
					Math.Min(i.x, min.x),
					Math.Min(i.y, min.y),
					Math.Min(i.z, min.z));
			}
			return min;
		}

		public Index limit(uint max) {
			return new Index(depth, Math.Min(x, max), Math.Min(y, max), Math.Min(z, max));
		}

		public override int GetHashCode() {
			long h = x ^ y ^ z ^ depth;
			h = (h^0xdeadbeef) + (h<<4);
			h = h ^ (h>>10);
			h = h + (h<<7);
			return (int)(h ^ (h>>13));
		}

		public override string ToString() {
			return "(" +depth +"; " +x +", " +y +", " +z +")";
		}
	}
}