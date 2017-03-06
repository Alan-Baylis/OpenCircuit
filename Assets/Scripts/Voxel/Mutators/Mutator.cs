using System.Collections.Generic;
using UnityEngine;

namespace Vox {

	public abstract class Mutator {

		public bool ignoreMasks = false;

		/// <summary>
		/// Applies mutator as configured to a voxel tree.
		/// </summary>
		/// <param name="target">the voxel tree to apply the mutator to</param>
		public void apply(OcTree target) {
			App app = init(target);
			//applyMasksToApplication(cache, target);

			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			watch.Start();
			apply(app, target.head, new Index());
			watch.Stop();
			MonoBehaviour.print("Mutator Apply Time: " +watch.Elapsed.TotalSeconds);
			foreach(VoxelJob job in app.jobs)
				job.execute();
			target.dirty = true;
		}

		/// <summary>
		/// Initializes an Application.
		/// </summary>
		/// <param name="app">the mutator application to initialize/setup</param>
		public virtual App init(OcTree tree) {
			return new App(tree);
		}

		public abstract Act checkMutation(App app, Index pos);

		public abstract Voxel mutate(App app, Index pos, Act act, Voxel original);

		protected void apply(App app, VoxelBlock block, Index pos) {
			Index cornerChild = pos.getChild();
			for(byte c = 0; c<VoxelBlock.CHILD_COUNT; ++c) {
				// TODO: use min and max to reduce number of values considered
				Index childPos = cornerChild.getNeighbor(c);
				Act action = checkMutation(app, childPos);

				// check if voxel is outside of modifier area
				if (!action.modify)
					continue;

				// check if voxel is inside of masked area
				Act maskAction = checkMasks(app.tree, childPos);
				if (!maskAction.modify)
					continue;

				// recurse or set full voxel
				if (canTraverse(childPos, app.tree) && (maskAction.doTraverse || action.doTraverse))
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

		protected Act checkMasks(OcTree tree, Index p) {
			// skip if no masks or if ignoring masks
			if (ignoreMasks || tree.masks == null)
				return new Act(false, true);

			// check against each mask
			int voxelSize = 1 << (tree.maxDepth - p.depth);
			Act action = new Act(false, true);
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
						return new Act(false, false);
				}
			}
			return action;
		}

		protected static int compareToVoxel(int pos, int voxelPos, int voxelSize) {
            int min = voxelPos * voxelSize;
			int max = min + voxelSize;
			return min >= pos ? -1: (max <= pos ? 1 : 0);
		}

		protected static bool canTraverse(Index pos, OcTree tree) {
			return pos.depth < tree.maxDepth;
		}

		protected static float calculateVoxelSize(App app, Index p) {
			return 1 << (app.tree.maxDepth - p.depth);
		}

		protected static Vector3 calculateDiff(Vector3 position, Index p, float voxelSize) {
			return new Vector3(p.x + 0.5f, p.y + 0.5f, p.z + 0.5f) * voxelSize -position;
		}

		/// <summary>
		/// Stores data about a specific application of a modifier to a voxel tree.
		/// </summary>
		public class App {
			public bool updateMesh;
			public Index min, max;
			public readonly OcTree tree;
			public List<VoxelJob> jobs;

			public App(OcTree tree) {
				this.tree = tree;
				updateMesh = false;
				min = Index.ZERO;
				max = Index.ZERO;
				jobs = new List<VoxelJob>();
			}
		}

		/// <summary>
		/// The action to take for a particular voxel in the tree. This means whether
		/// to traverse to a higher detail, modify the whole voxel, or leave it alone.
		/// </summary>
		public struct Act {
			public bool doTraverse;
			public bool modify;
			public object cache;

			public Act(bool doTraverse, bool modify) {
				this.doTraverse = doTraverse;
				this.modify = modify;
				cache = null;
			}

			public Act(object cache, bool modify=false, bool doTraverse=false) {
				this.doTraverse = doTraverse;
				this.modify = modify;
				this.cache = cache;
			}
		}

	}
}
