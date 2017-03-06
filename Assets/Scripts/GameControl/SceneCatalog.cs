using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[CreateAssetMenu(fileName = "SceneCatalog", menuName = "SceneCatalog")]
public class SceneCatalog : ScriptableObject {

	private static SceneCatalog myInstance;
	public static SceneCatalog sceneCatalog { get {
			if (myInstance == null) {
				myInstance = Resources.Load("SceneCatalog") as SceneCatalog;
			}
			return myInstance;
		}
	}

	public List<SceneData> sceneData = new List<SceneData>();

	public List<string> scenes = new List<string>();

	private Dictionary<string, SceneData> mySceneMap = null;

	private Dictionary<string, SceneData> sceneMap {
		get {
			if (mySceneMap == null) {
				mySceneMap = new Dictionary<string, SceneData>();
				foreach (SceneData data in sceneData) {
					mySceneMap.Add(data.path, data);
				}
			}
			return mySceneMap;
		}
	}

	public void clearSceneList() {
		scenes = new List<string>();
	}

	public void addScene(string path) {
		scenes.Add(path);
	}

	public string getScenePath(int index) {
		if (index >= 0 && index < scenes.Count ) {
			return scenes[index];
		}
		return null;
	}

	public void addSceneData(string path) {
		if (!sceneMap.ContainsKey(path)) {
			SceneData data = new SceneData(path, GlobalConfigData.getDefault());
			sceneData.Add(data);
			sceneMap.Add(path, data);
		}
	}

	public SceneData? getSceneData(string path) {
		if (sceneMap.ContainsKey(path))
			return sceneMap[path];
		return null;
	}


	public List<SceneData> getScenesForGameMode(GameMode.GameModes mode) {
		List<SceneData> results = new List<SceneData>();
		foreach (string scenePath in scenes) {
			if (sceneMap.ContainsKey(scenePath)) {
				SceneData data = sceneMap[scenePath];
				if (data.supportedGameModes.Contains(mode)) {
					results.Add(data);
				}
			}
		}
		return results;
	} 
}
