﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[InitializeOnLoad]
public class LevelLoad {

	static LevelLoad() {
//		freshInitialize();
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

		// create player
		createPrefab("Assets/Prefabs/Player.prefab");

		// create empty voxel object
		Vox.VoxelEditor.createEmpty();

		// create sun
		createPrefab("Assets/Prefabs/Sun.prefab");

		// create menu
		createPrefab("Assets/Prefabs/Main Menu.prefab");

		// set lighting mode
		Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
	}

	private static void createPrefab(string assetPath) {
		PrefabUtility.InstantiatePrefab(
			AssetDatabase.LoadAssetAtPath<GameObject>(assetPath));
	}
}
