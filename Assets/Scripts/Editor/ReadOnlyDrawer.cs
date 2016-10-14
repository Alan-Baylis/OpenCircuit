
using UnityEditor;
using UnityEngine;
using System.ComponentModel;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {

	public override void OnGUI(Rect position,
							   SerializedProperty property,
							   GUIContent label) {
		Object obj = property.serializedObject.targetObject;
		if (PrefabUtility.GetPrefabType(obj) != PrefabType.Prefab
			&& Application.isPlaying)
			GUI.enabled = false;
		EditorGUI.PropertyField(position, property, label, true);
		GUI.enabled = true;
	}
}