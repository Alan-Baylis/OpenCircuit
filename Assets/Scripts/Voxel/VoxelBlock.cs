using UnityEngine;
using System.IO;

namespace Vox {

	[ExecuteInEditMode]
	public class VoxelBlock : VoxelHolder {

		public static int totalConsolidations;
		public static int skippedSubdivisions;
		public const byte CHILD_COUNT_POWER = 1;
		public const int CHILD_DIMENSION = 1 << CHILD_COUNT_POWER;
		public const int CHILD_COUNT = CHILD_DIMENSION *CHILD_DIMENSION *CHILD_DIMENSION;

		public VoxelHolder[, ,] children;

		public VoxelBlock(Voxel fillValue) {
			children = new VoxelHolder[CHILD_DIMENSION, CHILD_DIMENSION, CHILD_DIMENSION];
			++blockCount;
			set(fillValue);
		}

		public VoxelBlock() : this(Voxel.empty) { }

		public VoxelBlock(BinaryReader reader) {
			children = new VoxelHolder[CHILD_DIMENSION, CHILD_DIMENSION, CHILD_DIMENSION];
			++blockCount;
			for (byte xi = 0; xi < CHILD_DIMENSION; ++xi)
				for (byte yi = 0; yi < CHILD_DIMENSION; ++yi)
					for (byte zi = 0; zi < CHILD_DIMENSION; ++zi)
						children[xi, yi, zi] = VoxelHolder.deserialize(reader);
		}

		public override void serialize(BinaryWriter writer) {
			writer.Write(VoxelHolder.VOXEL_BLOCK_SERIAL_ID);
			for (byte xi = 0; xi < CHILD_DIMENSION; ++xi)
				for (byte yi = 0; yi < CHILD_DIMENSION; ++yi)
					for (byte zi = 0; zi < CHILD_DIMENSION; ++zi)
						children[xi, yi, zi].serialize(writer);
		}

		public void set(Voxel fillValue) {
			for (byte xi = 0; xi < CHILD_DIMENSION; ++xi)
				for (byte yi = 0; yi < CHILD_DIMENSION; ++yi)
					for (byte zi = 0; zi < CHILD_DIMENSION; ++zi)
						children[xi, yi, zi] = fillValue;
		}

		public void set(byte detailLevel, int x, int y, int z, Voxel value, OcTree control) {
			if (detailLevel > 0) {
				short factor = (short)(1 << (detailLevel - CHILD_COUNT_POWER));
				byte xi = (byte)(x / factor);
				byte yi = (byte)(y / factor);
				byte zi = (byte)(z / factor);
				if (detailLevel == CHILD_COUNT_POWER) {
					children[xi, yi, zi] = value;
				} else {
					if (children[xi, yi, zi].GetType() == typeof(Voxel)) {
						if (children[xi, yi, zi].Equals(value)) { ++skippedSubdivisions; return; }
						children[xi, yi, zi] = new VoxelBlock((Voxel)children[xi, yi, zi]);
					}
					((VoxelBlock)children[xi, yi, zi]).set((byte)(detailLevel - CHILD_COUNT_POWER), x - xi * factor, y - yi * factor, z - zi * factor, value, control);
				}
			} else
				set(value);
		}
		
		public override VoxelHolder get(Index i) {
			return get(i.depth, i.x, i.y, i.z);
		}

		public override VoxelHolder get(byte detailLevel, uint x, uint y, uint z) {

			if (detailLevel > 0) {
				ushort factor = (ushort)(1 << (detailLevel - CHILD_COUNT_POWER));
				byte xi = (byte)(x / factor);
				byte yi = (byte)(y / factor);
				byte zi = (byte)(z / factor);
				if (detailLevel == CHILD_COUNT_POWER)
					return children[xi, yi, zi];
				return children[xi, yi, zi].get((byte)(detailLevel - CHILD_COUNT_POWER), (uint)(x - xi * factor), (uint)(y - yi * factor), (uint)(z - zi * factor));
			} else
				return this;
		}

		public override byte detail() {
			byte detail = 0;
			for (byte xi = 0; xi < CHILD_DIMENSION; ++xi)
				for (byte yi = 0; yi < CHILD_DIMENSION; ++yi)
					for (byte zi = 0; zi < CHILD_DIMENSION; ++zi)
						detail = (byte)Mathf.Max(detail, children[xi, yi, zi].detail());
			return (byte)(detail + 1);
		}

		public override byte averageOpacity() {
			short totalOpacity = 0;
			for (byte xi = 0; xi < CHILD_DIMENSION; ++xi)
				for (byte yi = 0; yi < CHILD_DIMENSION; ++yi)
					for (byte zi = 0; zi < CHILD_DIMENSION; ++zi)
						totalOpacity += children[xi, yi, zi].averageOpacity();

			return (byte)(totalOpacity / (float)(CHILD_DIMENSION << (CHILD_COUNT_POWER * 2)));
		}

		public override byte averageMaterialType() {
			short totalMats = 0;
			for (byte xi = 0; xi < CHILD_DIMENSION; ++xi)
				for (byte yi = 0; yi < CHILD_DIMENSION; ++yi)
					for (byte zi = 0; zi < CHILD_DIMENSION; ++zi)
						totalMats += children[xi, yi, zi].averageMaterialType();

			return (byte)(totalMats / (float)(CHILD_DIMENSION << (CHILD_COUNT_POWER * 2)) + 0.5f);
		}

		public override Voxel toVoxel() {
			return new Voxel(averageMaterialType(), averageOpacity());
		}

		public VoxelBlock expand(uint x, uint y, uint z) {
			if (children[x, y, z].GetType() == typeof(Voxel))
				children[x, y, z] = new VoxelBlock((Voxel)children[x, y, z]);
			return (VoxelBlock)children[x, y, z];
		}

		public void updateAll(Index index, OcTree control, bool force = false) {
			// check if this is a high enough detail level.  If not, call the childrens' update methods
			VoxelRenderer renderer = control.getRenderer(index);
			if (!isRenderDepth(index.depth, control)) {
				for (byte xi = 0; xi < CHILD_DIMENSION; ++xi) {
					for (byte yi = 0; yi < CHILD_DIMENSION; ++yi) {
						for (byte zi = 0; zi < CHILD_DIMENSION; ++zi) {
							if (children[xi, yi, zi].GetType() == typeof(Voxel)) {
								children[xi, yi, zi] = new VoxelBlock((Voxel)children[xi, yi, zi]);
							}
							Index subIndex = new Index((byte)(index.depth +1),
								(byte)(index.x * CHILD_DIMENSION + xi),
								(byte)(index.y * CHILD_DIMENSION + yi),
								(byte)(index.z * CHILD_DIMENSION + zi));
							UpdateCheckJob job = new UpdateCheckJob((VoxelBlock)children[xi, yi, zi], control, subIndex);
							control.enqueueCheck(job);
						}
					}
				}
				return;
			}

			// check if we already have a mesh
			if (renderer == null) {
				renderer = new VoxelRenderer(index, control);
			} else if (!force) {
				return;
			}

			// We should generate a mesh
			GenMeshJob updateJob = new GenMeshJob(this, control, index);
			control.enqueueUpdate(updateJob);
		}

		public static bool isRenderDepth(byte depth, OcTree control) {
			return control.renderDepth - VoxelRenderer.VOXEL_COUNT_POWER == depth;
		}

		public override int canSimplify(out Voxel simplification) {
			bool canSimplify = true;
			int count = 0;
			simplification = null;
			for (int xi = 0; xi<CHILD_DIMENSION; ++xi) {
				for (int yi = 0; yi<CHILD_DIMENSION; ++yi) {
					for (int zi = 0; zi<CHILD_DIMENSION; ++zi) {
						Voxel child;
						count += children[xi, yi, zi].canSimplify(out child);
						if (child != null) {
							children[xi, yi, zi] = child;
							if (simplification == null)
								simplification = child;
							else
								canSimplify = simplification == child && canSimplify;
						} else {
							canSimplify = false;
						}
                    }
				}
			}
			if (canSimplify)
				++count;
			else
				simplification = null;
			return count;
		}
		public override int cleanArtifacts(out Voxel simplified, VoxelHolder head, byte level, byte maxLevel, int x, int y, int z) {
			int size = 1 << (CHILD_COUNT_POWER *level -CHILD_COUNT_POWER);
			int count = 0;
			for (int xi = 0; xi<CHILD_DIMENSION; ++xi) {
				for (int yi = 0; yi<CHILD_DIMENSION; ++yi) {
					for (int zi = 0; zi<CHILD_DIMENSION; ++zi) {
						Voxel newChild = null;
						count += children[xi, yi, zi].cleanArtifacts(out newChild, head, (byte)(level +CHILD_COUNT_POWER), maxLevel, x +xi *size, y +yi *size, z +zi *size);
						if (newChild != null)
							children[xi, yi, zi] = newChild;
					}
				}
			}
			simplified = null;
			return count;
		}

		private static float getDistSquare(Vector3 otherPos, Vector3 myPos, float size) {
			return (otherPos - myPos * size).sqrMagnitude;
		}

	}

}