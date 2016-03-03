using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class ClientController : NetworkBehaviour {

	private static List<NetworkInstanceId> cameras = new List<NetworkInstanceId>();

	public GameObject playerPrefab;

	[SyncVar(hook="setIsAlive")]
	private bool shouldSpawn = true;
	[SyncVar(hook="setPlayerId")]
	private NetworkInstanceId id;
	private GameObject player = null;
	private bool spawnPending = false;

	[SyncVar]
	private bool isDead = false;

	private int currentCamera = -1;
	private Camera sceneCamera;

	[ClientCallback]
	void Start() {
		Camera[] cams = FindObjectsOfType<Camera>();
		foreach(Camera cam in cams) {
			if(cam.transform.parent == null) {
				sceneCamera = cam;
			}
		}

		Player[] players = FindObjectsOfType<Player>();
		foreach(Player player in players) {
			if(player.netId != id) {
				cameras.Add(player.netId);
			}
		}
	}

	[ClientCallback]
	void Update() {
		//if(isLocalPlayer || (player != null && player.GetComponent<Player>().isLocalPlayer)) {
		//	print("num cameras: " + cameras.Count);
		//}
		if(isLocalPlayer) {

			if(shouldSpawn) {
				if(!spawnPending) {
					spawnPending = true;
					spawnPlayer();
				}
			}

			if(isDead && Input.GetButtonDown("Use")) {
				switchCamera();
			}
		} else {
			if(isDead) {
				cameras.Remove(id);
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

	[Server]
	public void setPlayerDead() {
		shouldSpawn = false;
		isDead = true;
		NetworkServer.ReplacePlayerForConnection(connectionToClient, gameObject, playerControllerId);
		RpcSwitchCam();
	}

	[Server]
	public void setRespawned() {
		shouldSpawn = true;
		isDead = false;
	}

	[Client]
	private void spawnPlayer() {
		if(currentCamera >= 0) {
			disableCurrentCam();
			disableSceneCam();
			currentCamera = -1;
		}
		CmdSetPlayer();
	}

	[Client]
	private void setPlayerId(NetworkInstanceId id) {
		this.id = id;
		shouldSpawn = false;
		player = ClientScene.FindLocalObject(id);
		player.GetComponent<Player>().controller = this;
		spawnPending = false;
		if(player.GetComponent<Player>().isLocalPlayer) {
			disableSceneCam();
			player.GetComponentInChildren<Camera>().enabled = true;
			player.GetComponentInChildren<AudioListener>().enabled = true;
		} else {
			cameras.Add(id);
		}
	}

	[Client]
	private void setIsAlive(bool isAlive) {
		this.shouldSpawn = isAlive;
	}

	[ClientRpc]
	private void RpcSwitchCam() {
		switchCamera();
	}

	[Client]
	private void switchCamera() {
		if(!isLocalPlayer)
			return;
		print("switch cam: " + currentCamera);
		print("num cams: " + cameras.Count);
		if(currentCamera >= 0) {
			disableCurrentCam();
			incrementCamera();
		} else {
			currentCamera = 0;
		}
		enableCurrentCam();
	}

	[Client]
	private void incrementCamera() {
		if(currentCamera >= cameras.Count - 1) {
			currentCamera = 0;
		} else {
			currentCamera++;
		}
	}

	[Client]
	private void disableCurrentCam() {
		if(!isLocalPlayer)
			return;
		if(currentCamera < cameras.Count) {
			Camera actualCam = ClientScene.FindLocalObject(cameras[currentCamera]).GetComponentInChildren<Camera>();
			actualCam.enabled = false;
			actualCam.GetComponent<AudioListener>().enabled = false;
		}
	}

	[Client]
	private void disableSceneCam() {
		if(!isLocalPlayer && !player.GetComponent<Player>().isLocalPlayer)
			return;
		if(sceneCamera != null) {
			sceneCamera.enabled = false;
			sceneCamera.GetComponent<AudioListener>().enabled = false;
		}
	}

	[Client]
	private void enableCurrentCam() {
		if(!isLocalPlayer)
			return;
		print("we are the local player");
		if(currentCamera < cameras.Count) {
			print("cameras in bounds");
			Camera nextCam = ClientScene.FindLocalObject(cameras[currentCamera]).GetComponentInChildren<Camera>();
			if(nextCam != null) {
				print("enabling cam");
				nextCam.enabled = true;
				nextCam.GetComponent<AudioListener>().enabled = true;
			}
		}
	}
}
