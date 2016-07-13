using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[InitializeOnLoad]
public class LevelLoad {

	static LevelLoad() {
		EditorApplication.hierarchyWindowChanged += freshInitialize;
	}

	public static void freshInitialize() {
		// check that the level is fresh
		GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();
		if (gameObjects.Length != 2)
			return;
		List<GameObject> objectList = new List<GameObject>(gameObjects);
		if (objectList.Find(go => go.name == "Main Camera") == null ||
		    objectList.Find(go => go.name == "Directional Light") == null)
			return;
		
		// delete default objects
		foreach(GameObject ob in gameObjects)
			GameObject.DestroyImmediate(ob);

		// create organizer objects
		Transform gameControl = new GameObject("Game Control").transform;
		Transform environment = new GameObject("Environment").transform;


		// create network manager
		createPrefab("Assets/Prefabs/GameControl/NetworkManager.prefab", gameControl);

		// create game controller
		CentralRobotController crc = createPrefab("Assets/Prefabs/Robots/CRC.prefab", gameControl)
			.GetComponent<CentralRobotController>();

		// create game controller
		createPrefab("Assets/Prefabs/GameControl/GameController.prefab", gameControl)
			.GetComponent<GlobalConfig>().centralRobotController = crc;

		// create start point
		createPrefab("Assets/Prefabs/GameControl/StartPosition.prefab", gameControl);

		// create empty voxel object
		Vox.VoxelEditor.createEmpty().transform.parent = environment;

		// create sun
		createPrefab("Assets/Prefabs/Sun.prefab", environment);

		// create scene camera
		createPrefab("Assets/Prefabs/GameControl/SceneCamera.prefab", gameControl);

		// create menu
		createPrefab("Assets/Prefabs/Main Menu.prefab", gameControl);

		// set lighting mode
		Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
	}

	private static GameObject createPrefab(string assetPath) {
		return createPrefab(assetPath, null);
	}

	private static GameObject createPrefab(string assetPath, Transform parent) {
		GameObject prefab = PrefabUtility.InstantiatePrefab(
			AssetDatabase.LoadAssetAtPath<GameObject>(assetPath)) as GameObject;
		prefab.transform.parent = parent;
		return prefab;
	}
}
