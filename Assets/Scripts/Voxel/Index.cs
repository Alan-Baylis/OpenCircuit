
namespace Vox {
	
	[System.Serializable]
	public struct Index {

		public byte depth;
		public uint x, y, z;

		public uint xLocal {get {
			return x % VoxelBlock.CHILD_DIMENSION;
		}}
		public uint yLocal {get {
			return y % VoxelBlock.CHILD_DIMENSION;
		}}
		public uint zLocal {get {
			return z % VoxelBlock.CHILD_DIMENSION;
		}}

		public Index(byte depth) : this(depth, 0, 0, 0) {
		}

		public Index(byte depth, uint x, uint y, uint z) {
			this.depth = depth;
			this.x = x;
			this.y = y;
			this.z = z;
		}
		
		public Index getChild() {
			return new Index((byte)(depth +1), x*2, y*2, z*2);
		}

		public Index getChild(byte i) {
			return new Index((byte)(depth +1), (uint)(x*2 +((i &4) >> 2)), (uint)(y *2 +((i &2) >> 1)), (uint)(z *2 +(i &1)));
		}

		public Index getNeighbor(byte i) {
			return new Index(depth, (uint)(x +((i &4) >> 2)), (uint)(y +((i &2) >> 1)), (uint)(z +(i &1)));
		}

		public Index getNeighbor(int xOffset, int yOffset, int zOffset) {
			return new Index(depth, (uint)(x +xOffset), (uint)(y +yOffset), (uint)(z +zOffset));
		}

		public Index getParent(byte pDepth) {
			byte diff = (byte)(depth - pDepth);
			return new Index(pDepth, x >> diff, y >> diff, z >> diff);
		}

		public override bool Equals(object ob) {
			if (ob == null || GetType () != ob.GetType())
				return false;
			return this == (Index)ob;
		}

		public static bool operator==(Index v1, Index v2) {
			if (System.Object.ReferenceEquals(v1, v2))
				return true;
			if (((object)v1 == null) ^ ((object)v2 == null))
				return false;
			return (v1.x == v2.x && v1.y == v2.y && v1.z == v2.z && v1.depth == v2.depth);
		}
		
		public static bool operator!=(Index v1, Index v2) {
			return !(v1 == v2);
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