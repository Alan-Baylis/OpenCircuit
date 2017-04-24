using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TeamId), true)]
public class TeamGUI : Editor {

	public override void OnInspectorGUI() {
		serializedObject.Update();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("id"), true);

		serializedObject.ApplyModifiedProperties();

		if (Application.isPlaying && GlobalConfig.globalConfig != null) {
			TeamId teamId = (TeamId)target;
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.ColorField(teamId.team.config.color);
			EditorGUI.EndDisabledGroup();
		}
	}
}
