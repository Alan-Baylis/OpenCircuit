using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ClientController : NetworkBehaviour {

	public GameObject playerPrefab;

	[SyncVar]
	private bool isAlive = true;
	[SyncVar]
	private NetworkInstanceId id;
	private GameObject player = null;

	[ClientCallback]
	void Update() {
		if(!isLocalPlayer) {
			return;
		}
		if(isAlive && player == null) {
				disableOtherPlayerCameras();
				spawnPlayer();
		}
	}
	
	[Command]
	private void CmdSetPlayer(short playerId) {
		GameObject newPlayer = Instantiate(playerPrefab, transform.position, Quaternion.identity) as GameObject;
		newPlayer.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
		NetworkServer.SpawnWithClientAuthority(newPlayer, connectionToClient);
		NetworkServer.ReplacePlayerForConnection(connectionToClient, newPlayer, playerControllerId);
		id = newPlayer.GetComponent<NetworkIdentity>().netId;
	}

	public void setPlayerDead() {
		isAlive = false;
	}

	[Command]
	public void CmdSetRespawned() {
		isAlive = true;
	}

	public bool isClientPlayerAlive() {
		return isAlive;
	}

	private void spawnPlayer() {
		CmdSetPlayer(playerControllerId);
		player = ClientScene.FindLocalObject(id);
		player.GetComponentInChildren<Camera>().enabled = true;
		player.GetComponentInChildren<AudioListener>().enabled = true;
		player.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
		player.GetComponent<Player>().controller = this;
	}

	private void disableOtherPlayerCameras() {
		Camera[] cameras = FindObjectsOfType<Camera>();
		foreach(Camera cam in cameras) {
			cam.enabled = false;
		}
	}
}
