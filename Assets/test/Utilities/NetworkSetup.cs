using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

public class NetworkSetup : MonoBehaviour, IPrebuildSetup {

	private bool listening;

	public static RobotController robotControllerPrefab;

	void Awake() {
		robotControllerPrefab = AssetDatabase.LoadAssetAtPath<RobotController>("Assets/Prefabs/Robots/robot.prefab");
		if (!listening) {
			listening = GetComponent<NetworkController>().listen();
		}
	}

	public virtual void Setup() {
		setupNetwork();
		setupAudioListener();
	}

	private void setupAudioListener() {
		GameObject gameObject = new GameObject("AudioListener");
		gameObject.AddComponent<AudioListener>();
	}

	private void setupNetwork() {
		GameObject networkControllerObject = new GameObject("NetworkController");
		networkControllerObject.AddComponent<NetworkController>();
		networkControllerObject.AddComponent<NetworkSetup>();
		networkControllerObject.AddComponent<NetworkManager>();
	}
}
