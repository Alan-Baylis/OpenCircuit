using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System;

namespace Vox {

	[ExecuteInEditMode]
	public class VoxelBlock : VoxelHolder {

		public static int totalConsolidations = 0;
		public static int skippedSubdivisions = 0;
		public const byte CHILD_COUNT_POWER = 1;
		public const int CHILD_DIMENSION = 1 << (CHILD_COUNT_POWER);
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

		public void setToHeightmap(byte detailLevel, int x, int y, int z, ref float[,] map, byte material, OcTree control) {
			if (detailLevel <= CHILD_COUNT_POWER) {
				for (int xi = 0; xi < CHILD_DIMENSION; ++xi) {
					for (int zi = 0; zi < CHILD_DIMENSION; ++zi) {
						for (int yi = 0; yi < CHILD_DIMENSION; ++yi) {
							if (yi + y >= map[x + xi, z + zi])
								break;
							else if (material == byte.MaxValue) {
								children[xi, yi, zi] = Voxel.empty;
							} else {
								if (yi + y >= map[x + xi, z + zi] - 1) {
									byte opacity = (byte)((map[x + xi, z + zi] - yi - y) * byte.MaxValue);
									if (opacity > control.isoLevel || children[xi, yi, zi].averageOpacity() <= opacity)
										children[xi, yi, zi] = new Voxel(material, opacity);
								} else {
									children[xi, yi, zi] = new Voxel(material, byte.MaxValue);
								}
							}
						}
					}
				}
			} else {
				int multiplier = (1 << (detailLevel - CHILD_COUNT_POWER));
				for (int xi = 0; xi < CHILD_DIMENSION; ++xi) {
					for (int zi = 0; zi < CHILD_DIMENSION; ++zi) {
						int xMax = x + (xi + 1) * multiplier;
						int zMax = z + (zi + 1) * multiplier;
						float yMin = float.MaxValue;
						float yMax = 0;
						for (int xPos = x + xi * multiplier; xPos < xMax; ++xPos) {
							for (int zPos = z + zi * multiplier; zPos < zMax; ++zPos) {
								if (map[xPos, zPos] < yMin) yMin = map[xPos, zPos];
								if (map[xPos, zPos] > yMax) yMax = map[xPos, zPos];
							}
						}

						int firstUnsolidBlock = Mathf.Min(((int)(yMin - y)) / multiplier, CHILD_DIMENSION);
						int lastUnsolidBlock = Mathf.Min(((int)(yMax - y)) / multiplier, CHILD_DIMENSION - 1);
						int yi = 0;
						for (; yi < firstUnsolidBlock; ++yi) {
							if (material == byte.MaxValue)
								children[xi, yi, zi] = Voxel.empty;
							else
								children[xi, yi, zi] = new Voxel(material, byte.MaxValue);
						}
						if (lastUnsolidBlock < 0) continue;
						for (; yi <= lastUnsolidBlock; ++yi) {
							if (children[xi, yi, zi].GetType() == typeof(Voxel))
								children[xi, yi, zi] = new VoxelBlock((Voxel)children[xi, yi, zi]);
							((VoxelBlock)children[xi, yi, zi]).setToHeightmap((byte)(detailLevel - CHILD_COUNT_POWER), x + xi * multiplier, y + yi * multiplier, z + zi * multiplier, ref map, material, control);
						}
					}
				}
			}
			control.dirty = true;
		}

		public void setToHeightmap(byte detailLevel, int x, int y, int z, ref float[,] map, byte[,] mats, OcTree control) {
			if (detailLevel <= CHILD_COUNT_POWER) {
				for (int xi = 0; xi < CHILD_DIMENSION; ++xi) {
					for (int zi = 0; zi < CHILD_DIMENSION; ++zi) {
						for (int yi = 0; yi < CHILD_DIMENSION; ++yi) {
							if (yi + y >= map[x + xi, z + zi])
								break;
							else if (yi + y >= map[x + xi, z + zi] - 1) {
								if (mats[x + xi, z + zi] == byte.MaxValue)
									children[xi, yi, zi] = Voxel.empty;
								else
									children[xi, yi, zi] = new Voxel(mats[x + xi, z + zi], (byte)((map[x + xi, z + zi] - yi - y) * byte.MaxValue));
							} else {
								if (mats[x + xi, z + zi] == byte.MaxValue)
									children[xi, yi, zi] = Voxel.empty;
								else
									children[xi, yi, zi] = new Voxel(mats[x + xi, z + zi], byte.MaxValue);
							}
						}
					}
				}
			} else {
				int multiplier = (1 << (detailLevel - CHILD_COUNT_POWER));
				for (int xi = 0; xi < CHILD_DIMENSION; ++xi) {
					for (int zi = 0; zi < CHILD_DIMENSION; ++zi) {
						int xMax = x + (xi + 1) * multiplier;
						int zMax = z + (zi + 1) * multiplier;
						float yMin = float.MaxValue;
						float yMax = 0;
						bool multipleMaterials = false;
						byte material = mats[x, z];
						for (int xPos = x + xi * multiplier; xPos < xMax; ++xPos) {
							for (int zPos = z + zi * multiplier; zPos < zMax; ++zPos) {
								if (map[xPos, zPos] < yMin) yMin = map[xPos, zPos];
								if (map[xPos, zPos] > yMax) yMax = map[xPos, zPos];
								if (mats[xPos, zPos] != material) multipleMaterials = true;
							}
						}

						if (multipleMaterials) yMin = 0;
						int firstUnsolidBlock = Mathf.Min(((int)(yMin - y)) / multiplier, CHILD_DIMENSION);
						int lastUnsolidBlock = Mathf.Min(((int)(yMax - y)) / multiplier, CHILD_DIMENSION - 1);
						int yi = 0;
						for (; yi < firstUnsolidBlock; ++yi) {
							if (mats[x + xi * multiplier, z + zi * multiplier] == byte.MaxValue)
								children[xi, yi, zi] = Voxel.empty;
							else
								children[xi, yi, zi] = new Voxel(mats[x + xi * multiplier, z + zi * multiplier], byte.MaxValue);
						}
						if (lastUnsolidBlock < 0) continue;
						for (; yi <= lastUnsolidBlock; ++yi) {
							VoxelBlock newChild = new VoxelBlock();
							newChild.setToHeightmap((byte)(detailLevel - CHILD_COUNT_POWER), x + xi * multiplier, y + yi * multiplier, z + zi * multiplier, ref map, mats, control);
							children[xi, yi, zi] = newChild;
						}
					}
				}
			}
			control.dirty = true;
		}

		public void updateAll(uint x, uint y, uint z, byte detailLevel, OcTree control, bool force = false) {
			// check if this is a high enough detail level.  If not, call the childrens' update methods
			VoxelRenderer renderer = control.getRenderer(new Index(detailLevel, x, y, z));
			if (!isRenderSize(control.sizes[detailLevel], control) && (!isRenderLod(x, y, z, control.sizes[detailLevel], control))) {
				for (byte xi = 0; xi < CHILD_DIMENSION; ++xi) {
					for (byte yi = 0; yi < CHILD_DIMENSION; ++yi) {
						for (byte zi = 0; zi < CHILD_DIMENSION; ++zi) {
							if (children[xi, yi, zi].GetType() == typeof(Voxel)) {
								children[xi, yi, zi] = new VoxelBlock((Voxel)children[xi, yi, zi]);
							}
							UpdateCheckJob job = new UpdateCheckJob((VoxelBlock)children[xi, yi, zi], control, (byte)(detailLevel + 1));
							job.setOffset((byte)(x * CHILD_DIMENSION + xi), (byte)(y * CHILD_DIMENSION + yi), (byte)(z * CHILD_DIMENSION + zi));
							control.enqueueCheck(job);
						}
					}
				}
				return;
			}

			// check if we already have a mesh
			if (renderer == null) {
				renderer = new VoxelRenderer(new Index(detailLevel, x, y, z), control);
			} else if (!force) {
				return;
			}

			// We should generate a mesh
			GenMeshJob updateJob = new GenMeshJob(this, control, detailLevel);
			updateJob.setOffset(x, y, z);
			control.enqueueUpdate(updateJob);
		}

		public static bool isRenderSize(float size, OcTree control) {
			return control.sizes[control.maximumDetail - VoxelRenderer.VOXEL_COUNT_POWER] == size;
		}

		public static bool isRenderLod(float x, float y, float z, float size, OcTree control) {
			if (!control.useLod)
				return size == control.sizes[control.maximumDetail];
			return getDistSquare(control.getLocalCamPosition(), new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), size) >= size * size * control.getLodDetail();
		}

		public override void putInArray(ref Voxel[,,] array, Index position, uint xMin, uint yMin, uint zMin, uint xMax, uint yMax, uint zMax) {
			uint size = 1u << (CHILD_COUNT_POWER *position.depth -CHILD_COUNT_POWER);
			for(uint xi=0; xi<CHILD_DIMENSION; ++xi) {
				uint xPos = position.x +xi *size;
				if (xPos > xMax || xPos +size < xMin)
					continue;
				for (uint yi=0; yi<CHILD_DIMENSION; ++yi) {
					uint yPos = position.y +yi *size;
					if (yPos > yMax || yPos +size < yMin)
						continue;
					for (uint zi=0; zi<CHILD_DIMENSION; ++zi) {
						uint zPos = position.z +zi *size;
						if (zPos > zMax || zPos +size < zMin)
							continue;
						children[xi, yi, zi].putInArray(ref array, new Index((byte)(position.depth -1), xPos, yPos, zPos), xMin, yMin, zMin, xMax, yMax, zMax);
					}
				}
			}
		}

		public override int canSimplify(out Voxel simplification) {
			bool canSimplify = true;
			int count = 0;
			simplification = null;
			for (int xi = 0; xi<CHILD_DIMENSION; ++xi) {
				for (int yi = 0; yi<CHILD_DIMENSION; ++yi) {
					for (int zi = 0; zi<CHILD_DIMENSION; ++zi) {
						Voxel child = null;
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
			//return Mathf.Max(Mathf.Max(Mathf.Abs(dif.x) - size * 0.5f, Mathf.Abs(dif.y) - size * 0.5f), Mathf.Abs(dif.z) - size * 0.5f);
		}

	}

}