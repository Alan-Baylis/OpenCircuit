using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour, SceneLoadListener {

	public static SceneLoader sceneLoader;

	private AsyncOperation async;
	private bool loading;

	private string nextScenePath;

	private bool delayLoad = true;
	private float loadDelaySeconds = 0f;
    private SceneLoadListener sceneLoadListener;

    // Use this for initialization
	void Start() {
		DontDestroyOnLoad(gameObject);
		sceneLoader = this;

		SceneData ? activeScene = SceneCatalog.sceneCatalog.getSceneData(SceneManager.GetActiveScene().path);

		if (activeScene == null || activeScene.Value.isLoadingScene()) {
			loadScene(1, this);
			Menu.menu.activeAtStart = false;
		} else {
			Menu.menu.activeAtStart = true;
		}
	}

	// Update is called once per frame
	void Update () {
		if (async != null && loading && async.progress >= .9f && !async.isDone) {
			ActivateScene();
		} else if (async != null && loading && async.isDone) {
			async = null;
			loading = false;
		    if (sceneLoadListener != null) {
		        sceneLoadListener.onSceneLoaded();
		    }
		}
	}

	public void loadScene(int index, SceneLoadListener listener) {
	    if (index >= 0 && index < SceneCatalog.sceneCatalog.scenes.Count) {
	        string path = SceneCatalog.sceneCatalog.scenes[index];
	        loadScene(path, listener);
	    } else {
	        Debug.LogError("Attempted to load scene " + index + " when the total scene count is only " +
	                       SceneManager.sceneCountInBuildSettings + "!");
	    }
	}

	public void loadScene(string path, SceneLoadListener listener) {
	    sceneLoadListener = listener;
		nextScenePath = path;
        cutToLoadScene();
		StartCoroutine("loadByPath");
	}

	IEnumerator loadByPath() {
	    yield return new WaitForSeconds(loadDelaySeconds);
        loading = true;
        async = SceneManager.LoadSceneAsync(nextScenePath);
        async.allowSceneActivation = false;

		yield return async;
	}

	public void ActivateScene() {
		async.allowSceneActivation = true;
	}

    private void cutToLoadScene() {
        string scenePath = SceneCatalog.sceneCatalog.getScenePath(0);
        SceneData ? sceneData = SceneCatalog.sceneCatalog.getSceneData(scenePath);
		if (sceneData != null && sceneData.Value.isLoadingScene() && !SceneManager.GetActiveScene().path.Equals(sceneData.Value.path)) {
			SceneManager.LoadScene(0);
		}
    }

    public void onSceneLoaded() {
        Menu.menu.activeAtStart = true;
    }
}
