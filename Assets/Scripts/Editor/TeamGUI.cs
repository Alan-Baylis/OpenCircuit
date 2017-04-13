using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Team), true)]
public class TeamGUI : Editor {

	public override void OnInspectorGUI() {
		serializedObject.Update();
		Team team = (Team) target;
		team.autoInitializeTeam = EditorGUILayout.Toggle("Auto Initialize", team.autoInitializeTeam);
		if (team.autoInitializeTeam) {
			EditorGUILayout.PropertyField(serializedObject.FindProperty("teamIndex"));
		}
		if (!team.autoInitializeTeam || Application.isPlaying){
			EditorGUILayout.PropertyField(serializedObject.FindProperty("team"), true);
		}

		serializedObject.ApplyModifiedProperties();
	}
}
