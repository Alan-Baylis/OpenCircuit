using UnityEngine;
using UnityEngine.Networking;

public class DeathZone : NetworkBehaviour {

	[ServerCallback]
	public void OnTriggerEnter(Collider other) {
		Player player = other.gameObject.GetComponent<Player>();
		if (player == null)
			return;
		player.die();
	}
}
