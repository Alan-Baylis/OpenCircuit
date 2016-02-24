using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ClientController : NetworkBehaviour {

	public GameObject playerPrefab;

	[SyncVar]
	private bool isAlive = true;
	[SyncVar(hook="setPlayerId")]
	private NetworkInstanceId id;
	private GameObject player = null;
	private bool spawnPending = false;

	private short clientId;

	void Start() {
		clientId = playerControllerId;
	}

	[ClientCallback]
	void Update() {
		if(!isLocalPlayer) {
			return;
		}
		if(isAlive && player == null) {
			if(!spawnPending) {
				disableOtherPlayerCameras();
				spawnPlayer();
				spawnPending = true;
			}
		}
	}
	
	[Command]
	private void CmdSetPlayer() {
		GameObject newPlayer = Instantiate(playerPrefab, transform.position, Quaternion.identity) as GameObject;
		newPlayer.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
		NetworkServer.SpawnWithClientAuthority(newPlayer, connectionToClient);
		NetworkServer.ReplacePlayerForConnection(connectionToClient, newPlayer, playerControllerId);
		id = newPlayer.GetComponent<NetworkIdentity>().netId;
	}

	public void setPlayerDead() {
		isAlive = false;
		RpcEnableOtherPlayerCameras();
	}

	
	public void setRespawned() {
		isAlive = true;
	}

	public bool isClientPlayerAlive() {
		return isAlive;
	}

	private void spawnPlayer() {
		CmdSetPlayer();
	}

	private void setPlayerId(NetworkInstanceId id) {
		this.id = id;
		player = ClientScene.FindLocalObject(id);
		player.GetComponent<Player>().controller = this;
		spawnPending = false;
		if(player.GetComponent<Player>().isLocalPlayer) {
			player.GetComponentInChildren<Camera>().enabled = true;
			player.GetComponentInChildren<AudioListener>().enabled = true;
		}
	}

	[ClientRpc]
	private void RpcEnableOtherPlayerCameras() {
		if(!isLocalPlayer)
			return;
		Camera[] cams = FindObjectsOfType<Camera>();
		foreach(Camera cam in cams) {
			if(cam != null && !cam.enabled) {
				cam.enabled = true;
			}
		}
	}
	
	private void disableOtherPlayerCameras() {
		Camera[] cameras = FindObjectsOfType<Camera>();
		foreach(Camera cam in cameras) {
			cam.enabled = false;
		}
	}
}
