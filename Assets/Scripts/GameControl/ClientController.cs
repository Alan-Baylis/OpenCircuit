using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class ClientController : NetworkBehaviour {

	private static List<NetworkInstanceId> cameras = new List<NetworkInstanceId>();

	public GameObject playerPrefab;

	[SyncVar(hook="setPlayerId")]
	private NetworkInstanceId id;
	private GameObject player = null;

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
		disableSceneCam();
		if(isLocalPlayer) {
			CmdSpawnPlayerAt(transform.position);
		}
	}

	[ClientCallback]
	void Update() {
		if(isLocalPlayer) {
			if(isDead && Input.GetButtonDown("Use")) {
				switchCamera();
			}
		} else {
			if(isDead) {
				cameras.Remove(id);
			}
		}
	}

	//anyone can call!!
	public bool isAlive() {
		return !isDead;
	}

	[Command]
	private void CmdSpawnPlayerAt(Vector3 position) {
		spawnPlayerAt(position);
	}
	
	[Server]
	private void spawnPlayerAt(Vector3 position) {
		GameObject newPlayer = Instantiate(playerPrefab, position, Quaternion.identity) as GameObject;
		newPlayer.name = "player" + Random.Range(1, 20);
		NetworkServer.Spawn(newPlayer);
		NetworkServer.ReplacePlayerForConnection(connectionToClient, newPlayer, playerControllerId);
		id = newPlayer.GetComponent<NetworkIdentity>().netId;
	}

	[Server]
	public void setPlayerDead() {
		isDead = true;
		NetworkServer.ReplacePlayerForConnection(connectionToClient, gameObject, playerControllerId);
		RpcSwitchCam();
		enableSceneCam();
		FindObjectOfType<Menu>().lose();
	}

	[Server]
	public void respawnPlayerAt(Vector3 position) {
		if(isDead) {
			isDead = false;
			RpcResetCamera();
			spawnPlayerAt(position);
		}
	}

	[ClientRpc]
	private void RpcResetCamera() {
		if(currentCamera >= 0) {
			disableCurrentCam();
			disableSceneCam();
			currentCamera = -1;
		}
	}

	[Client]
	private void setPlayerId(NetworkInstanceId id) {
		this.id = id;
		player = ClientScene.FindLocalObject(id);
		player.GetComponent<Player>().controller = this;
		if(player.GetComponent<Player>().isLocalPlayer) {
			disableSceneCam();
			player.GetComponentInChildren<Camera>().enabled = true;
			player.GetComponentInChildren<AudioListener>().enabled = true;
		} else {
			cameras.Add(id);
		}
	}

	[ClientRpc]
	private void RpcSwitchCam() {
		//switchCamera();
		enableSceneCam();
	}

	[Client]
	private void switchCamera() {
		if(!isLocalPlayer)
			return;
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
	private void enableSceneCam() {
		if(!isLocalPlayer && !player.GetComponent<Player>().isLocalPlayer)
			return;
		if(sceneCamera != null) {
			sceneCamera.enabled = true;
			sceneCamera.GetComponent<AudioListener>().enabled = true;
		}
	}

	[Client]
	private void enableCurrentCam() {
		if(!isLocalPlayer)
			return;
		if(currentCamera < cameras.Count) {
			Camera nextCam = ClientScene.FindLocalObject(cameras[currentCamera]).GetComponentInChildren<Camera>();
			if(nextCam != null) {
				nextCam.enabled = true;
				nextCam.GetComponent<AudioListener>().enabled = true;
			}
		} else if(cameras.Count < 1) {
			print("enable scene cam");
			enableSceneCam();
		}
	}
}
