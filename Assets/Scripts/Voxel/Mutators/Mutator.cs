using System.Collections.Generic;

namespace Vox {

	public abstract class Mutator {

		/// <summary>
		/// Applies mutator as configured to a voxel tree.
		/// </summary>
		/// <param name="target">the voxel tree to apply the mutator to</param>
		public void apply(OcTree target) {
			Application app = setup(target);
			//applyMasksToApplication(app, target);

			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			watch.Start();
			apply(app, target.head, new Index());
			watch.Stop();
			UnityEngine.MonoBehaviour.print("Mutator Apply Time: " +watch.Elapsed.TotalSeconds);
			foreach(VoxelJob job in app.jobs)
				job.execute();
			target.dirty = true;
		}

		protected void apply(Application app, VoxelBlock block, Index pos) {
			Index cornerChild = pos.getChild();
			for(byte c = 0; c<VoxelBlock.CHILD_COUNT; ++c) {
				// TODO: use min and max to reduce number of values considered
				Index childPos = cornerChild.getNeighbor(c);
				Action action = checkMutation(app, childPos);

				// check if voxel is outside of modifier area
				if (!action.modify)
					continue;

				// check if voxel is inside of masked area
				Action maskAction = checkMasks(app.tree, childPos);
				if (!maskAction.modify)
					continue;

				// recurse or set full voxel
				if (childPos.depth < app.tree.maxDepth && (maskAction.doTraverse || action.doTraverse))
					apply(app, block.expand(childPos.xLocal, childPos.yLocal, childPos.zLocal), childPos);
				else
					block.children[childPos.xLocal, childPos.yLocal, childPos.zLocal] =
						mutate(app, childPos, action, block.children[childPos.xLocal, childPos.yLocal, childPos.zLocal].toVoxel());

				// update meshes if appropriate
				if (childPos.depth == app.tree.maxDepth - VoxelRenderer.VOXEL_COUNT_POWER) {
					UpdateCheckJob job = new UpdateCheckJob(block, app.tree, childPos);
					job.setForce(true);
					app.jobs.Add(job);
				}
			}
		}

		/// <summary>
		/// Sets up an Application.
		/// </summary>
		/// <param name="target">the voxel tree to setup for applying to</param>
		/// <returns>the setup Application</returns>
		public virtual Application setup(OcTree target) {
			Application app = new Application();
			app.tree = target;
			//uint width = (uint)(1 << (target.maximumDetail)) - 1;
			//app.min = new Index(target.maxDetail);
			//app.max = new Index(target.maxDetail, width, width, width);
			return app;
		}

		protected abstract Action checkMutation(Application app, Index pos);

		protected abstract Voxel mutate(Application app, Index pos, Action action, Voxel original);

		protected Action checkMasks(OcTree tree, Index p) {
			// skip if no masks
			if (tree.masks == null)
				return new Action(false, true);

			// check against each mask
			int voxelSize = 1 << (tree.maxDepth - p.depth);
			Action action = new Action(false, true); 
			foreach (VoxelMask mask in tree.masks) {
				if (mask.active) {

					// calculate relative position
					int comparison = (mask.maskAbove)?
						compareToVoxel((int)mask.yPosition, (int)p.y, voxelSize):
						-compareToVoxel((int)mask.yPosition, (int)p.y, voxelSize);

					// evaluate action based on relative position
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

		/// <summary>
		/// Stores data about a specific application of a modifier to a voxel tree.
		/// </summary>
		public class Application {
			public bool updateMesh;
			public Index min, max;
			public OcTree tree;
			public List<VoxelJob> jobs = new List<VoxelJob>();
		}

		/// <summary>
		/// The action to take for a particular voxel in the tree. This means Whether
		/// to traverse to a lower detail, modify the whole voxel, or leave it alone.
		/// </summary>
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
