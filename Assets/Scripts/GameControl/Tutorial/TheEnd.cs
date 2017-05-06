using UnityEngine;
using UnityEngine.Networking;

public class TheEnd : NetworkBehaviour {

	public Tutorial tutorial;

	[ServerCallback]
	private void OnTriggerEnter(Collider other) {
		Player player = other.transform.root.GetComponent<Player>();
		if (player != null) {
			player.GetComponent<Score>().value = 0;
			ClientController controller = player.clientController;
			player.die();
			controller.startTime = Time.time;
			RpcDoTheThing(controller.netId);

			if(isServer)
				controller.clientType = NetworkController.ClientType.PLAYER;
		}
	}

	[ClientRpc]
	private void RpcDoTheThing(NetworkInstanceId playerId) {
		ClientController player = ClientScene.FindLocalObject(playerId).GetComponent<ClientController>();
		if (player.isLocalPlayer) {
			tutorial.nextMessage();
			player.startTime = Time.time;
		}
	}
}
