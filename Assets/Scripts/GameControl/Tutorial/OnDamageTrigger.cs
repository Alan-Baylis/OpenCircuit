using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OnDamageTrigger : NetworkBehaviour {

	public Tutorial tutorial;
	public AbstractPlayerSpawner spawner;
	public float timer = .5f;

	private Queue<Player> playersQueue = new Queue<Player>();

	[Server]
	public void doTheThing(Player player) {
		if (playersQueue.Contains(player))
			return;
		playersQueue.Enqueue(player);
		StartCoroutine("teleportPlayer");
	}

	IEnumerator teleportPlayer() {
		yield return new WaitForSeconds(timer);
		Vector3 pos = spawner.nextSpawnPos();
		Player player = playersQueue.Dequeue();
		player.transform.position = pos;
		RpcDoTheThing(player.netId, pos);
	}

	[ClientRpc]
	private void RpcDoTheThing(NetworkInstanceId playerId, Vector3 pos) {
		Player player = ClientScene.FindLocalObject(playerId).GetComponent<Player>();
		if (player.isLocalPlayer) {
			player.transform.position = pos;
			tutorial.nextMessage();
		}
	}
}
