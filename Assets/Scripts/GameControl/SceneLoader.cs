using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class SceneLoader : MonoBehaviour {

	public static SceneLoader sceneLoader;

	public Menu menuPrefab;
	public NetworkManager networkManagerPrefab;
	public GlobalConfig gameControllerPrefab;

	private AsyncOperation async;
	private bool loading = false;
	private Menu menu = null;
	private List<Scene> scenes = new List<Scene>();

	private int nextScene;
	private string nextScenePath;

	private bool delayLoad = true;
	private float loadDelaySeconds = 10f;
	private float timer = 0f;

	// Use this for initialization
	void Start() {
		DontDestroyOnLoad(gameObject);
		sceneLoader = this;
		if (NetworkManager.singleton == null) {
			Instantiate(networkManagerPrefab.gameObject, Vector3.zero, Quaternion.identity);
		}
		DontDestroyOnLoad(Instantiate(gameControllerPrefab.gameObject, Vector3.zero, Quaternion.identity));
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
		if (loading) {
			timer += Time.deltaTime;
			//print("timer: " + timer);
		}
		if (async != null && loading && async.progress >= .9f && !async.isDone && timer > loadDelaySeconds ) {
			Debug.Log("activate scene after " + timer + " seconds.");
			ActivateScene();
		} else if (async != null && loading && async.isDone) {
			async = null;
			loading = false;
			GlobalConfig.globalConfig.startGame();
			SceneCatalog sceneCatalog = SceneCatalog.sceneCatalog;
			SceneData? sceneData = sceneCatalog.getSceneData(SceneManager.GetActiveScene().path);
			if (sceneData != null)
				menu.serverConfig = sceneData.Value.configuration;
			menu.activeAtStart = true;
		}
	}

	public void loadScene(int index) {
		timer = 0;
		nextScene = index;
		string scenePath = SceneCatalog.sceneCatalog.getScenePath(0);
		SceneData? sceneData = SceneCatalog.sceneCatalog.getSceneData(scenePath);
		if (sceneData != null && sceneData.Value.isLoadingScene() && !SceneManager.GetActiveScene().path.Equals(sceneData.Value.path)) {
			SceneManager.LoadScene(0);
		}
		StartCoroutine("load");
	}

	public void loadScene(string path) {
		timer = 0;
		nextScenePath = path;
		string scenePath = SceneCatalog.sceneCatalog.getScenePath(0);
		SceneData ? sceneData = SceneCatalog.sceneCatalog.getSceneData(scenePath);
		if (sceneData != null && sceneData.Value.isLoadingScene() && !SceneManager.GetActiveScene().path.Equals(sceneData.Value.path)) {
			SceneManager.LoadScene(0);
		}
		StartCoroutine("loadByPath");
	}

	IEnumerator load() {
		if (nextScene < SceneManager.sceneCountInBuildSettings) {
			loading = true;
			async = SceneManager.LoadSceneAsync(nextScene);
			async.allowSceneActivation = false;
		} else {
			Debug.LogError("Attempted to load scene " + nextScene + " when the total scene count is only " + SceneManager.sceneCountInBuildSettings + "!");
		}
		yield return async;
	}

	IEnumerator loadByPath() {
		if (nextScene < SceneManager.sceneCountInBuildSettings) {
			loading = true;
			async = SceneManager.LoadSceneAsync(nextScenePath);
			async.allowSceneActivation = false;
		} else {
			Debug.LogError("Attempted to load scene '" + nextScenePath + "' but it is not in the build!");
		}
		yield return async;
	}

	public void ActivateScene() {
		async.allowSceneActivation = true;
	}
}
