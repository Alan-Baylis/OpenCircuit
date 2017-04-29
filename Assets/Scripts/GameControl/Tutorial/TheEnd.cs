using UnityEngine;
using UnityEngine.Networking;

public class TheEnd : NetworkBehaviour {

	private void OnTriggerEnter(Collider other) {
		Player player = other.transform.root.GetComponent<Player>();
		if (player != null && player.isLocalPlayer) {
			player.GetComponent<Score>().value = 0;
			player.die();
		}

		player.clientController.startTime = Time.time;

		if(isServer)
			player.clientController.clientType = NetworkController.ClientType.PLAYER;

	}
}
