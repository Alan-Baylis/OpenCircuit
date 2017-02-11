

namespace Vox {
	public class ArrayReader : Reader<Voxel[,,]> {

		private Index min;
		private Index max;

		public ArrayReader(Index min, uint xWidth, uint yWidth, uint zWidth) {
			this.min = min.bounded;
			this.max = new Index(min.depth, min.x +xWidth, min.y +yWidth, min.z +zWidth).bounded;
		}

		public ArrayReader(Index min, uint width) : this(min, width, width, width) {}

		public ArrayReader(Index min, Index max) {
			Index.normalizeDepth(ref min, ref max);
			this.min = min.bounded;
			this.max = max.bounded;
		}

		protected override Reading setup(OcTree target) {
			return new ArrayReading(target, max -min);
		}

		protected override Voxel[,,] read(Reading reading) {
			ArrayReading aReading = reading as ArrayReading;
			read(aReading, reading.tree.head, Index.ZERO);
			return aReading.array;
		}

		protected void read(ArrayReading reading, VoxelBlock block, Index pos) {
			uint size = 1u << (VoxelBlock.CHILD_COUNT_POWER *(min.depth - pos.depth - 1));
			for (byte i=0; i<VoxelBlock.CHILD_COUNT; ++i) {
				Index childPos = pos.getChild(i);

				if (childPos.x *size >= max.x || (childPos.x +1) *size <= min.x ||
				    childPos.y *size >= max.y || (childPos.y +1) *size <= min.y ||
				    childPos.z *size >= max.z || (childPos.z +1) *size <= min.z ) {
					continue;
				}

				VoxelHolder child = block.children[childPos.xLocal, childPos.yLocal, childPos.zLocal];
				if (childPos.depth < min.depth && child is VoxelBlock) {
					read(reading, child as VoxelBlock, childPos);
				} else {
					readVoxel(reading, child, childPos);
				}
			}
		}

		protected void readVoxel(ArrayReading reading, VoxelHolder voxel, Index position) {
			Voxel value = voxel.toVoxel();
			if (position.depth == min.depth) {
				Index i = position - min;
				reading.array[i.x, i.y, i.z] = value;
				return;
			}
			Index start = Index.maxMerge(position, min) -min;
			Index end = Index.minMerge(position.getNeighbor(1, 1, 1), max) -min;
			for(uint xi=start.x; xi<end.x; ++xi) {
				for(uint yi=start.y; yi<end.y; ++yi) {
					for(uint zi=start.z; zi<end.z; ++zi) {
						reading.array[xi, yi, zi] = value;
					}
				}
			}
		}

		protected class ArrayReading : Reading {

			public Voxel[,,] array;

			public ArrayReading(OcTree tree, Index width) : base(tree) {
				array = new Voxel[width.x, width.y, width.z];
			}

		}
	}
}