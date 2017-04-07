using UnityEditor;

[CustomEditor(typeof(GlobalConfig), true)]
public class GameControllerGUI : Editor {


    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("playerPrefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("configuration"), true, null);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraManager"));
        serializedObject.ApplyModifiedProperties();
    }
}