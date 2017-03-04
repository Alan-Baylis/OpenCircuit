using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class SceneLoader : MonoBehaviour {

	public static SceneLoader sceneLoader;

	public Menu menuPrefab;
	public NetworkManager networkManagerPrefab;

	private AsyncOperation async;
	private bool loading = false;
	private Menu menu = null;
	private List<Scene> scenes = new List<Scene>();

	private int nextScene;

	// Use this for initialization
	void Start() {
		DontDestroyOnLoad(gameObject);
		sceneLoader = this;
		if (NetworkManager.singleton == null) {
			Instantiate(networkManagerPrefab.gameObject, Vector3.zero, Quaternion.identity);
		}
		menu = Instantiate(menuPrefab, Vector3.zero, Quaternion.identity) as Menu;
		DontDestroyOnLoad(menu.gameObject);

		SceneData ? activeScene = SceneCatalog.sceneCatalog.getSceneData(SceneManager.GetActiveScene().path);

		if (activeScene == null || activeScene.Value.isLoadingScene()) {
			loadScene(1);
			menu.activeAtStart = false;
		} else {
			menu.activeAtStart = true;
		}
	}

	// Update is called once per frame
	void Update () {
		if (async != null && loading && async.progress >= .9f && !async.isDone) {
			ActivateScene();
		} else if (async != null && loading && async.isDone) {
			loading = false;
			menu.activeAtStart = true;
		}
	}

	public void loadScene(int index) {
		nextScene = index;
		StartCoroutine("load");
	}

	IEnumerator load() {
		if (nextScene < SceneManager.sceneCountInBuildSettings) {
			loading = true;
			Debug.LogWarning("ASYNC LOAD STARTED - " +
			   "DO NOT EXIT PLAY MODE UNTIL SCENE LOADS... UNITY WILL CRASH");
			async = SceneManager.LoadSceneAsync(nextScene);
			//async = Sc.LoadLevelAsync(1);
			async.allowSceneActivation = false;
		} else {
			Debug.LogError("Attempted to load scene " + nextScene + " when the total scene count is only " + SceneManager.sceneCountInBuildSettings + "!");
		}
		yield return async;
	}

	public void ActivateScene() {
		async.allowSceneActivation = true;
	}
}
