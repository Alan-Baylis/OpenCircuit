using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;


public class SceneListWriter {

	[PostProcessBuildAttribute(0)]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
		SceneCatalog sceneCatalog = AssetDatabase.LoadAssetAtPath("Assets/Resources/SceneCatalog.asset", typeof(SceneCatalog)) as SceneCatalog;
		EditorUtility.SetDirty(sceneCatalog);
		sceneCatalog.clearSceneList();
		foreach (EditorBuildSettingsScene Scene in EditorBuildSettings.scenes) {
			sceneCatalog.addScene(Scene.path);
		}
	}


}
