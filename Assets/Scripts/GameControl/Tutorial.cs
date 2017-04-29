using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Tutorial : NetworkBehaviour {
	public List<string> toolTips = new List<string>();
	private List<Player> players = new List<Player>();

	[Server]
	public void addPlayer(Player player) {
		players.Add(player);
		RpcFreezePlayer(player.netId);
	}

	[ClientRpc]
	private void RpcFreezePlayer(NetworkInstanceId playerId) {
		GameObject playerGameObject = ClientScene.FindLocalObject(playerId);
		playerGameObject.GetComponent<Player>().frozen = true;
	}

}
