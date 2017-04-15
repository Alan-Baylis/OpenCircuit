using UnityEditor;

[CustomEditor(typeof(TeamId), true)]
public class TeamGUI : Editor {

	public override void OnInspectorGUI() {
		serializedObject.Update();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("id"), true);

		serializedObject.ApplyModifiedProperties();
	}
}
