using UnityEngine;
using UnityEngine.Networking;

public class TutorialTrigger : NetworkBehaviour {

	public Tutorial tutorial;

	private Player player;
	public float timer = .25f;


	[ServerCallback]
	private void OnTriggerEnter(Collider other) {
		player = other.transform.root.GetComponent<Player>();
		if (player != null) {
			player.inventory.unequip();
			RpcDoTheThing(player.netId);
		}
	}

	[ClientRpc]
	private void RpcDoTheThing(NetworkInstanceId playerId) {
		Player player = ClientScene.FindLocalObject(playerId).GetComponent<Player>();
		if (player.isLocalPlayer) {
			if (player.inventory != null) {
				player.inventory.unequip();
			} else
				Debug.LogError("NULL PLAYER INVENTORY!!!!");
			tutorial.nextMessage();
		}
	}
}
