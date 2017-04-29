using System.Collections.Generic;
using UnityEngine;
using Vox;

public class BrushGroup : MonoBehaviour {

	public VoxelEditor voxelEditor;
	public VoxelEditor.BrushType brushType;

	public void apply() {
		applyBrushGroups();
		applyBrushes();
	}

	private Vector3[] getBrushPoints() {
		List<Vector3> points = new List<Vector3>();
		for (int i = 0; i < transform.childCount; ++i) {
			if (transform.GetChild(i).GetComponent<BrushGroup>() == null) {
				points.Add(transform.GetChild(i).position);
			}
		}
		return points.ToArray();
	}

	private void applyBrushes() {
		Vox.LocalMutator mut = (Vox.LocalMutator)buildMutator(voxelEditor, transform.GetChild(0).position);
		if (transform.childCount > 1) {


			new Vox.LineMutator(getBrushPoints(), mut).apply(voxelEditor);
		}
	}

	private void applyBrushGroups() {
		for (int i = 0; i < transform.childCount; ++i) {
			BrushGroup group = transform.GetChild(i).GetComponent<BrushGroup>();
			if (group != null) {
				group.apply();
			}
		}
	}

	void OnDrawGizmos() {
		if (voxelEditor.ghostBrushAlpha > 0) {
			Color brushColor = voxelEditor.getBrushColor();

			// draw path points
			if (transform.childCount > 0 && voxelEditor.isSelectedBrushPathable()) {
				Vector3[] points = getBrushPoints();
				for(int i=0; i < points.Length; ++i) {
					Gizmos.color = brushColor;
					drawBrushGizmo(points[i]);
					drawPathPoint(points[i]);
				}
				Gizmos.color = Color.yellow;
				for (int i = 0; i < points.Length -1; ++i) {
					Gizmos.DrawLine(points[i], points[i +1]);
				}
			}
		}
		if (voxelEditor.maskDisplayAlpha > 0 && voxelEditor.masks != null) {
			Gizmos.color = new Color(1, 0, 0, voxelEditor.maskDisplayAlpha);
			foreach (VoxelMask mask in voxelEditor.masks) {
				if (!mask.active)
					continue;
				Gizmos.DrawMesh(generateRectangleMesh(new Vector3(voxelEditor.width, 0, voxelEditor.width)), transform.TransformPoint(voxelEditor.width / 2, mask.yPosition * voxelEditor.voxelSize(), voxelEditor.width / 2));
			}
			Gizmos.color = Color.gray;
		}
	}

	private Vox.Mutator buildMutator(Vox.VoxelEditor editor, Vector3 point) {
		// check for subtraction mode
		byte opacity = byte.MaxValue;
		if (editor.isSubtracting()) {
			opacity = byte.MinValue;
		}

		// create mutator (and maybe apply)
		switch (brushType) {
			case VoxelEditor.BrushType.Sphere:
				Vox.SphereMutator sphereMod = new Vox.SphereMutator(point, editor.sphereBrushSize, new Vox.Voxel(editor.sphereBrushSubstance, opacity));
				sphereMod.overwriteShape = !editor.sphereSubstanceOnly;
				return sphereMod;
			case VoxelEditor.BrushType.Rectangle:
				Vox.CubeMutator cubeMod = new Vox.CubeMutator(editor, point, editor.cubeBrushDimensions, new Vox.Voxel(editor.cubeBrushSubstance, opacity), true);
				cubeMod.overwriteShape = !editor.cubeSubstanceOnly;
				return cubeMod;
			default:
				Vox.BlurMutator blurMod = new Vox.BlurMutator(editor, point, editor.smoothBrushSize, editor.smoothBrushStrength);
				blurMod.blurRadius = editor.smoothBrushBlurRadius;
				return blurMod;
		}
	}

	private void drawPathPoint(Vector3 point) {
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(point, new Vector3(0.5f, 0.5f, 0.5f) *voxelEditor.voxelSize());
	}

	private void drawBrushGizmo(Vector3 location) {
		switch (brushType) {
			case VoxelEditor.BrushType.Sphere:
				Gizmos.DrawSphere(location, voxelEditor.sphereBrushSize);
				break;
			case VoxelEditor.BrushType.Rectangle:
				Gizmos.DrawMesh(generateRectangleMesh(voxelEditor.cubeBrushDimensions), location);
				break;
			case VoxelEditor.BrushType.Smooth:
				Gizmos.DrawSphere(location, voxelEditor.smoothBrushSize);
				break;
		}
	}

	private Mesh generateRectangleMesh(Vector3 scale) {
		Mesh mesh = new Mesh();
		scale = scale / 2;
		Vector3[] vertices = new Vector3[] {
			new Vector3(-scale.x, -scale.y, -scale.z),
			new Vector3( scale.x, -scale.y, -scale.z),
			new Vector3(-scale.x,  scale.y, -scale.z),
			new Vector3( scale.x,  scale.y, -scale.z),
			new Vector3(-scale.x, -scale.y,  scale.z),
			new Vector3( scale.x, -scale.y,  scale.z),
			new Vector3(-scale.x,  scale.y,  scale.z),
			new Vector3( scale.x,  scale.y,  scale.z),
		};
		mesh.vertices = vertices;
		mesh.normals = new Vector3[vertices.Length];
		mesh.triangles = new int[] {
			0, 1, 5,
			5, 4, 0,
			2, 7, 3,
			7, 2, 6,
			0, 3, 1,
			2, 3, 0,
			4, 5, 7,
			6, 4, 7,
			1, 3, 5,
			5, 3, 7,
			0, 4, 2,
			4, 6, 2,
		};
		return mesh;
	}
}
