

namespace Vox {
	public abstract class Reader<T> {

		/// <summary>
		/// Reads data from a voxel tree.
		/// </summary>
		/// <param name="target">the voxel tree to read data from</param>
		/// <returns>the data read from the voxel tree</returns>
		public T read(OcTree target) {
			Reading reading = setup(target);
			return read(reading);
		}

		protected virtual Reading setup(OcTree target) {
			return new Reading(target);
		}

		protected abstract T read(Reading reading);

		protected class Reading {
			public OcTree tree;

			public Reading(OcTree tree) {
				this.tree = tree;
			}
		}

	}
}
