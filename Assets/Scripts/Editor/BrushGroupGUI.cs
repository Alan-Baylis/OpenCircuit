using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BrushGroup))]
public class BrushGroupGUI : Editor {

	public override void OnInspectorGUI() {
		serializedObject.Update();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("voxelEditor"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("brushType"));

		serializedObject.ApplyModifiedProperties();

		if (GUILayout.Button("Apply Brush Group")) {
			((BrushGroup)target).apply();
		}
	}
}
