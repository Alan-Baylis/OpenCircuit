using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class CheckPoint : NetworkBehaviour {

	[ServerCallback]
	public void OnTriggerEnter(Collider other) {
		Player player = other.GetComponent<Player>();
		if(player != null) {
			ClientController [] controllers = FindObjectsOfType<ClientController>();
			foreach(ClientController controller in controllers) {
				print("respawn player");
				controller.respawnPlayer();
			}
		}
	}
}
