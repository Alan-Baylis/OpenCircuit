using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class PlayModeTestUtility {

	private static GameObject navMeshObject;

	public static RobotController createScratchRobot() {
		GameObject gameObject = new GameObject();
		gameObject.AddComponent<NavMeshAgent>();
		return gameObject.AddComponent<RobotController>();
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
