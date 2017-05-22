using UnityEngine;
using UnityEngine.Networking;

public class EnablePlayerGunTrigger : NetworkBehaviour {

	public Tutorial tutorial;

	[ServerCallback]
	private void OnTriggerEnter(Collider other) {
		Player player = other.transform.root.GetComponent<Player>();
		if (player != null) {
			AssaultRifle rifle = (AssaultRifle)player.inventory.getItemsExtending<AbstractGun>()[0];
			rifle.clearAmmo();
			player.inventory.equip(rifle);

			RpcDoTheThing(player.netId, rifle.netId);
		}
	}

	[ClientRpc]
	void RpcDoTheThing(NetworkInstanceId player, NetworkInstanceId rifle) {
		Player curPlayer = ClientScene.FindLocalObject(player).GetComponent<Player>();
		if (curPlayer.isLocalPlayer) {
			curPlayer.inventory.equip(ClientScene.FindLocalObject(rifle).GetComponent<AssaultRifle>());
			tutorial.nextMessage();
		}
	}
}
