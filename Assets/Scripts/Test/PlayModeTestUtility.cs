using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class PlayModeTestUtility {

	private static GameObject navMeshObject;

	public static GlobalConfig createGlobalConfig<T>() where T : GameMode {
		GameObject globalConfigObject = new GameObject("GlobalConfig");
		GlobalConfig globalConfig = globalConfigObject.AddComponent<GlobalConfig>();
		globalConfig.gamemode = globalConfigObject.AddComponent<T>();
		globalConfig.cameraManager = globalConfigObject.AddComponent<CameraManager>();
		return globalConfig;
	}

	public static RobotController createScratchRobot() {
		GameObject gameObject = new GameObject();
		gameObject.AddComponent<NavMeshAgent>();
		return gameObject.AddComponent<RobotController>();
	}

	public static Player createPlayer() {
		GameObject playerObject = new GameObject("Player");
		playerObject.AddComponent<NetworkIdentity>();
		playerObject.AddComponent<BoxCollider>();
		return playerObject.AddComponent<Player>();
	}

	public static ClientController createClientController() {
		GameObject clientControllerObject = new GameObject("ClientController");
		return clientControllerObject.AddComponent<ClientController>();
	}

	public static Score createScore() {
		GameObject gameObject = new GameObject("");
		gameObject.AddComponent<Label>();
		return gameObject.AddComponent<Score>();
	}

	public static RobotController createRobot() {
		return MonoBehaviour.Instantiate(NetworkSetup.robotControllerPrefab, Vector3.zero, Quaternion.identity);
	}

	public static T addRobotComponent<T>(RobotController robot) where T : Component {
		GameObject gameObject = new GameObject();
		gameObject.transform.parent = robot.transform;
		return gameObject.AddComponent<T>();
	}

	public static void setupNavMesh() {
		if (navMeshObject == null) {
			navMeshObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
			List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
			NavMeshBuilder.CollectSources(navMeshObject.transform, NavMesh.AllAreas, NavMeshCollectGeometry.PhysicsColliders,
				GameObjectUtility.GetNavMeshAreaFromName("Walkable"), new List<NavMeshBuildMarkup>(), sources);
			NavMeshBuildSettings settings = new NavMeshBuildSettings();
			NavMesh.AddNavMeshData(NavMeshBuilder.BuildNavMeshData(settings, sources,
				new Bounds(navMeshObject.transform.position, new Vector3(20, 20, 20)), navMeshObject.transform.position,
				Quaternion.identity));
		}
	}
}
