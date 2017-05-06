using UnityEngine;
using UnityEngine.Networking;

public class AmmoPickupTrigger : NetworkBehaviour {
	public OnDamageTrigger odt;
	public Tutorial tutorial;
	private bool done;

	[ServerCallback]
	private void OnTriggerEnter(Collider other) {
		Player player = other.transform.root.GetComponent<Player>();
		if (player != null) {
			RpcDoTheThing(player.netId);
		}
	}

	[ClientRpc]
	private void RpcDoTheThing(NetworkInstanceId playerId) {
		if (ClientScene.FindLocalObject(playerId).GetComponent<Player>().isLocalPlayer && !done) {
			tutorial.nextMessage();
			done = true;
		}
	}
}
