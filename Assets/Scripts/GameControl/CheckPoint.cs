using UnityEngine;
using System.Collections;

public class CheckPoint : MonoBehaviour {
	public void OnTriggerEnter(Collider other) {
		Player player = other.GetComponent<Player>();
		if(player != null) {
			ClientController [] controllers = FindObjectsOfType<ClientController>();
			foreach(ClientController controller in controllers) {
				print("respawn player");
				controller.setRespawned();
			}
		}
	}
}
