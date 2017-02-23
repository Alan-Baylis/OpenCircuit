using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[CustomEditor(typeof(GlobalConfig), true)]
public class GameControllerGUI : Editor {


    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("configuration"), true, null);
        EditorGUILayout.Popup("test", 0, new string[] {"option 1", "option 2"}, new GUILayoutOption[]{});
        EditorGUILayout.PropertyField(serializedObject.FindProperty("centralRobotController"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("frozenPlayers"));
        serializedObject.ApplyModifiedProperties();
    }
}