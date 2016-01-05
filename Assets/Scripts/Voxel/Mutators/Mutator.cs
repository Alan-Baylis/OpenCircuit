

namespace Vox {

	public abstract class Mutator {

		protected uint maskMinY;
		protected uint maskMaxY;

		public void apply(Tree target) {
			Application app = setup(target);
			//applyMasksToApplication(app, target);
			apply(app, target.getHead(), new Index());
			target.dirty = true;
		}

		protected void apply(Application app, VoxelBlock block, Index pos) {
			Index cornerChild = pos.getChild();
			for(byte c = 0; c<VoxelBlock.CHILD_COUNT; ++c) {
				// TODO: use min and max to reduce number of values considered
				Index childPos = cornerChild.getNeighbor(c);
				Action action = checkMutation(app, childPos);
				if (!action.modify)
					continue;
				Action maskAction = checkMasks(app.tree, pos);
				if (!maskAction.modify)
					continue;
				if (childPos.depth < app.tree.maxDetail && (maskAction.doTraverse || action.doTraverse))
					apply(app, block.expand(childPos.xLocal, childPos.yLocal, childPos.zLocal), childPos);
				else
					block.children[childPos.xLocal, childPos.yLocal, childPos.zLocal] =
						mutate(app, childPos, action, block.children[childPos.xLocal, childPos.yLocal, childPos.zLocal].toVoxel());
				if (childPos.depth == app.tree.maxDetail - VoxelRenderer.VOXEL_COUNT_POWER && (action.modify))
					block.updateAll(childPos.x, childPos.y, childPos.z, childPos.depth, app.tree, true);
			}
		}

		//protected void apply(Application app) {
		//	maskMinY = uint.MinValue;
		//	maskMaxY = uint.MaxValue;
		//	if (app.tree.masks != null) {
		//		foreach (VoxelMask mask in app.tree.masks) {
		//			if (mask.active) {
		//				if (mask.maskAbove) {
		//					if (maskMaxY > mask.yPosition)
		//						maskMaxY = mask.yPosition;
		//				} else if (maskMinY < mask.yPosition) {
		//					maskMinY = mask.yPosition;
		//				}
		//			}
		//		}
		//	}
		//	maskMaxY -= 1;
		//	traverse(app.tree.getBaseUpdateInfo(), app.tree.maxDetail);
		//	app.tree.dirty = true;
		//}

		public virtual Application setup(Tree target) {
			Application app = new Application();
			uint width = (uint) (1 << (target.maxDetail)) - 1;
			app.tree = target;
			//app.min = new Index(target.maxDetail);
			//app.max = new Index(target.maxDetail, width, width, width);
			return app;
		}

		protected abstract Action checkMutation(Application app, Index pos);

		protected abstract Voxel mutate(Application app, Index pos, Action action, Voxel original);

		//protected void setMinMax(Vector3 min, Vector3 max) {
		//	minX = (int)(min.x + 0.01f);
		//	minY = (int)(min.y + 0.01f);
		//	minZ = (int)(min.z + 0.01f);
		//	maxX = (int)(max.x + 0.01f);
		//	maxY = (int)(max.y + 0.01f);
		//	maxZ = (int)(max.z + 0.01f);
		//}

		//protected void traverse(VoxelUpdateInfo info, byte detailLevel) {
		//	int factor = 1 << (detailLevel - VoxelBlock.CHILD_COUNT_POWER);
		//	byte xiMin = (byte)Mathf.Max(minX / factor - info.x * VoxelBlock.CHILD_DIMENSION, 0f);
		//	byte xiMax = (byte)Mathf.Min((maxX + 1) / factor - info.x * VoxelBlock.CHILD_DIMENSION, VoxelBlock.CHILD_DIMENSION - 1f);
		//	byte yiMin = (byte)Mathf.Max(minY / factor - info.y * VoxelBlock.CHILD_DIMENSION, 0f);
		//	byte yiMax = (byte)Mathf.Min((maxY + 1) / factor - info.y * VoxelBlock.CHILD_DIMENSION, VoxelBlock.CHILD_DIMENSION - 1f);
		//	byte ziMin = (byte)Mathf.Max(minZ / factor - info.z * VoxelBlock.CHILD_DIMENSION, 0f);
		//	byte ziMax = (byte)Mathf.Min((maxZ + 1) / factor - info.z * VoxelBlock.CHILD_DIMENSION, VoxelBlock.CHILD_DIMENSION - 1f);

		//	VoxelBlock block = (VoxelBlock)info.blocks[1, 1, 1];

		//	uint scale = (uint) (1 << (VoxelBlock.CHILD_COUNT_POWER *(detailLevel -1)));

		//	for (byte yi = yiMin; yi <= yiMax; ++yi) {

		//		if ((info.y *VoxelBlock.CHILD_DIMENSION +yi) < maskMinY /scale ||
		//		    (info.y *VoxelBlock.CHILD_DIMENSION +yi) > maskMaxY /scale +1) {
		//			continue;
		//		}

		//		for (byte xi = xiMin; xi <= xiMax; ++xi) {
		//			for (byte zi = ziMin; zi <= ziMax; ++zi) {
		//				if (detailLevel <= VoxelBlock.CHILD_COUNT_POWER) {
		//					block.children[xi, yi, zi] = modifyVoxel(block.children[xi, yi, zi], info.x * VoxelBlock.CHILD_DIMENSION + xi, info.y * VoxelBlock.CHILD_DIMENSION + yi, info.z * VoxelBlock.CHILD_DIMENSION + zi);
		//				} else {
		//					if (block.children[xi, yi, zi].GetType() == typeof(Voxel)) {
		//						block.children[xi, yi, zi] = new VoxelBlock((Voxel)block.children[xi, yi, zi]);
		//					}
		//					traverse(new VoxelUpdateInfo(info, xi, yi, zi), (byte)(detailLevel - VoxelBlock.CHILD_COUNT_POWER));
		//				}
		//			}
		//		}
		//	}

		//	if (updateMesh && info != null && (VoxelBlock.isRenderSize(info.size, control) || VoxelBlock.isRenderLod(info.x, info.y, info.z, info.size, control))) {
		//		//block.clearSubRenderers(control);
		//		block.updateAll(info.x, info.y, info.z, info.detailLevel, control, true);
		//	}
		//}

		//protected abstract VoxelHolder modifyVoxel(VoxelHolder original, int x, int y, int z);

		protected Action checkMasks(Tree tree, Index p) {
			if (tree.masks == null)
				return new Action(false, true);
			int voxelSize = 1 << (tree.maxDetail - p.depth);
			Action action = new Action(false, true);
			foreach (VoxelMask mask in tree.masks) {
				if (mask.active) {
					int comparison;
					if (mask.maskAbove) {
						comparison = compareToVoxel(-(int)mask.yPosition, -(int)p.y, voxelSize);
						if (maskMaxY > mask.yPosition)
							maskMaxY = mask.yPosition;
					} else {
						comparison = compareToVoxel((int)mask.yPosition, (int)p.y, voxelSize);
					}
					if (comparison == 0)
						action.doTraverse = true;
					else if (comparison < 0)
						return new Action(false, false);
				}
			}
			return action;
		}

		protected static int compareToVoxel(int pos, int voxelPos, int voxelSize) {
            int min = voxelPos * voxelSize;
			int max = min + voxelSize;
			return min > pos ? max <= pos ? 1 : -1 : 0;
		}

		public class Application {
			public bool updateMesh;
			public Index min, max;
			public Tree tree;
		}

		public class Action {
			public bool doTraverse;
			public bool modify;
			public Action(bool doTraverse, bool modify) {
				this.doTraverse = doTraverse;
				this.modify = modify;
			}
		}

	}
}
