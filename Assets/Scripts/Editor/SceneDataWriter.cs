#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class SceneDataWriter : UnityEditor.AssetModificationProcessor { 

	static string[] OnWillSaveAssets(string[] paths) {
		foreach (string path in paths) {
			if (path.EndsWith(".unity")) {
				SceneCatalog sceneCatalog = AssetDatabase.LoadAssetAtPath("Assets/Resources/SceneCatalog.asset", typeof(SceneCatalog)) as SceneCatalog;
				sceneCatalog.addScene(path);
			}
		}
		return paths;
	}
}
#endif
