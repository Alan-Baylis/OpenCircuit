using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

public class NetworkSetup : MonoBehaviour, IPrebuildSetup {

	void Awake() {
		GetComponent<NetworkController>().listen();
	}

	public void Setup() {
		GameObject networkControllerObject = new GameObject();
		networkControllerObject.AddComponent<NetworkController>();
		networkControllerObject.AddComponent<NetworkSetup>();
		networkControllerObject.AddComponent<NetworkManager>();
	}
}
