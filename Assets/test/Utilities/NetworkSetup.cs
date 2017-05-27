using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

public class NetworkSetup : MonoBehaviour, IPrebuildSetup {

	private bool listening;

	public static NetworkSetup instance;

	void Awake() {
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
		instance = networkControllerObject.AddComponent<NetworkSetup>();
		networkControllerObject.AddComponent<NetworkManager>();
	}
}
