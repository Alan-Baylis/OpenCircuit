using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Teleport : NetworkBehaviour {

	public AbstractPlayerSpawner target;
	public float timer = .25f;
	private Player player;

	[ServerCallback]
	private void OnTriggerEnter(Collider other) {
		player = other.transform.root.GetComponent<Player>();
		if (player != null) {
			StartCoroutine("teleportPlayer");
		}
	}

	IEnumerator teleportPlayer() {
		yield return new WaitForSeconds(timer);
		Vector3 pos = target.nextSpawnPos();
		player.transform.position = pos;
		RpcTeleportPlayer(player.netId, pos);
		player = null;
	}

	[ClientRpc]
	private void RpcTeleportPlayer(NetworkInstanceId playerId, Vector3 pos) {
		Player player = ClientScene.FindLocalObject(playerId).GetComponent<Player>();
		player.transform.position = pos;
	}
}
