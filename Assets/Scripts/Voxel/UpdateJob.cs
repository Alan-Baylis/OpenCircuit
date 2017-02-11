
namespace Vox {

	public class GenMeshJob : VoxelJob {

		private readonly Index index;
		private readonly VoxelBlock block;
		private readonly OcTree control;

		public GenMeshJob(VoxelBlock block, OcTree control, Index index) {
			this.block = block;
			this.control = control;
			this.index = index;
		}

		public override void execute() {
			lock (control) {
				System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
				watch.Start();

				Voxel[,,] array = new ArrayReader(index.getLevel(control.renderDepth),
					VoxelRenderer.VOXEL_DIMENSION +1).read(control);
				getRenderer().genMesh(array);

				watch.Stop();

				control.meshGenTime += watch.Elapsed.TotalSeconds;
				++control.meshGenCount;
			}
		}

		public VoxelBlock getBlock() {
			return block;
		}

		public VoxelRenderer getRenderer() {
			return control.getRenderer(index);
        }
	}

	public class ApplyMeshJob : VoxelJob {

		private VoxelRenderer rend;

		public ApplyMeshJob(VoxelRenderer rend) {
			this.rend = rend;
		}

		public override void execute() {
			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			watch.Start();
			if (VoxelBlock.isRenderDepth(rend.index.depth, rend.control))
				rend.applyMesh();
			watch.Stop();
			lock(rend.control) {
				rend.control.meshApplyTime += watch.Elapsed.TotalSeconds;
				++rend.control.meshApplyCount;
			}
		}
	}

	public class UpdateCheckJob : VoxelJob {

		public Index index;
        public bool force = false;
		private VoxelBlock block;
		private OcTree control;
		
		public UpdateCheckJob(VoxelBlock block, OcTree control, Index index) {
			this.block = block;
			this.control = control;
			this.index = index;
			control.addUpdateCheckJob();
		}

		public override void execute() {
			lock (control) {
				block.updateAll(index, control, force);
				control.removeUpdateCheckJob();
			}
		}

		public void setForce(bool force) {
			this.force = force;
		}
	}

	public class DropRendererJob : VoxelJob {

		private VoxelRenderer rend;
		
		public DropRendererJob(VoxelRenderer rend) {
			this.rend = rend;
		}

		public override void execute() {
			lock (rend.control) {
				rend.clear();
			}
		}
	}

	public class LinkRenderersJob: VoxelJob {
		private OcTree control;

		public LinkRenderersJob(OcTree control) {
			this.control = control;
		}

		public override void execute() {
			control.relinkRenderers();
		}
	}

}
