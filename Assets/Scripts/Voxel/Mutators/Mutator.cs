

namespace Vox {

	public abstract class Mutator {

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
				Action maskAction = checkMasks(app.tree, childPos);
				if (!maskAction.modify)
					continue;
				if (childPos.depth < app.tree.maximumDetail && (maskAction.doTraverse || action.doTraverse))
					apply(app, block.expand(childPos.xLocal, childPos.yLocal, childPos.zLocal), childPos);
				else
					block.children[childPos.xLocal, childPos.yLocal, childPos.zLocal] =
						mutate(app, childPos, action, block.children[childPos.xLocal, childPos.yLocal, childPos.zLocal].toVoxel());
				if (childPos.depth == app.tree.maximumDetail - VoxelRenderer.VOXEL_COUNT_POWER && (action.modify))
					block.updateAll(childPos.x, childPos.y, childPos.z, childPos.depth, app.tree, true);
			}
		}

		public virtual Application setup(Tree target) {
			Application app = new Application();
			uint width = (uint) (1 << (target.maximumDetail)) - 1;
			app.tree = target;
			//app.min = new Index(target.maxDetail);
			//app.max = new Index(target.maxDetail, width, width, width);
			return app;
		}

		protected abstract Action checkMutation(Application app, Index pos);

		protected abstract Voxel mutate(Application app, Index pos, Action action, Voxel original);

		protected Action checkMasks(Tree tree, Index p) {
			if (tree.masks == null)
				return new Action(false, true);
			int voxelSize = 1 << (tree.maximumDetail - p.depth);
			Action action = new Action(false, true);
			foreach (VoxelMask mask in tree.masks) {
				if (mask.active) {
					int comparison;
					if (mask.maskAbove) {
						comparison = compareToVoxel((int)mask.yPosition, (int)p.y, voxelSize);
					} else {
						comparison = -compareToVoxel((int)mask.yPosition, (int)p.y, voxelSize);
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
			return min >= pos ? -1: (max <= pos ? 1 : 0);
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
