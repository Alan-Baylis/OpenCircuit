using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

public class NetworkSetup : MonoBehaviour, IPrebuildSetup {

	private bool listening;

	public static RobotController robotControllerPrefab;
	public static RoboEyes RoboEyesPrefab;

	void Awake() {
		robotControllerPrefab = AssetDatabase.LoadAssetAtPath<RobotController>("Assets/Prefabs/Robots/robot.prefab");
		RoboEyesPrefab = AssetDatabase.LoadAssetAtPath<RoboEyes>("Assets/Prefabs/Robots/Eyes.prefab");
		if (!listening) {
			listening = GetComponent<NetworkController>().listen();
		}
	}

	public virtual void Setup() {
		setupNetwork();
		setupSceneCamera();
	}

	private void setupSceneCamera() {
		GameObject gameObject = new GameObject("SceneCamera");
		gameObject.AddComponent<AudioListener>();
		gameObject.AddComponent<Camera>();
		gameObject.tag = "SceneCamera";
	}

	private void setupNetwork() {
		GameObject networkControllerObject = new GameObject("NetworkController");
		networkControllerObject.AddComponent<NetworkController>();
		networkControllerObject.AddComponent<NetworkSetup>();
		networkControllerObject.AddComponent<NetworkManager>();
	}
}
