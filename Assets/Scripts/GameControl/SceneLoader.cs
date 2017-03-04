using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {

	public static SceneLoader sceneLoader;

	public Menu menuPrefab;

	private AsyncOperation async;
	private bool loading = false;
	private Menu menu = null;
	private List<Scene> scenes = new List<Scene>();

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(gameObject);
		sceneLoader = this;

		menu = Instantiate(menuPrefab, Vector3.zero, Quaternion.identity) as Menu;
		menu.activeAtStart = false;
		DontDestroyOnLoad(menu.gameObject);
		StartCoroutine("load");
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

	IEnumerator load() {
		loading = true;
		Debug.LogWarning("ASYNC LOAD STARTED - " +
		   "DO NOT EXIT PLAY MODE UNTIL SCENE LOADS... UNITY WILL CRASH");
		async = SceneManager.LoadSceneAsync(1);
		//async = Sc.LoadLevelAsync(1);
		async.allowSceneActivation = false;
		yield return async;
	}

	public void ActivateScene() {
		async.allowSceneActivation = true;
	}

	private void loadSceneData() {
		for (int i = 0; i < SceneManager.sceneCountInBuildSettings; ++i) {
			Scene scene = SceneManager.GetSceneByBuildIndex(i);
			scenes.Add(scene);
		}
	}
}
