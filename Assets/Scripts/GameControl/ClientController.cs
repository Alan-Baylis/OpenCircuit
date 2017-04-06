using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class ClientController : NetworkBehaviour {

	private static List<NetworkInstanceId> cameras = new List<NetworkInstanceId>();

	public GameObject playerPrefab;
	public GameObject playerCamPrefab;
	public GameObject playerLegsPrefab;
	public GameObject playerArmsPrefab;
    public static int numPlayers = 0;

	[SyncVar(hook="setPlayerId")]
	private NetworkInstanceId id;
	[SyncVar(hook="setCamId")]
	private NetworkInstanceId camId;
	[SyncVar(hook="setLegsId")]
	private NetworkInstanceId legsId;
	private GameObject player;

	[SyncVar(hook="setPlayerDead")]
	private bool isDead;

	private int currentCamera = -1;
	private Camera sceneCamera;

	[ClientCallback]
	void Start() {
		Camera[] cams = FindObjectsOfType<Camera>();
		foreach(Camera cam in cams) {
			if(cam.tag == "SceneCamera") {
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
        numPlayers++;
	}
	
	[Server]
	private void spawnPlayerAt(Vector3 position) {
		playerPrefab.SetActive(false);
		GameObject newPlayer = Instantiate(playerPrefab, position, Quaternion.identity);
		GameObject playerCam = Instantiate(playerCamPrefab, position, Quaternion.identity);
		GameObject playerLegs = Instantiate(playerLegsPrefab, position, Quaternion.identity);
		GameObject playerArms = Instantiate(playerArmsPrefab, position, Quaternion.identity);

		Player playerScript = newPlayer.GetComponent<Player>();
		playerScript.clientController = this;

		newPlayer.SetActive(true);

		playerCam.transform.parent = newPlayer.transform;
		playerCam.transform.localPosition = playerCamPrefab.transform.localPosition;
		playerLegs.transform.parent = newPlayer.transform;
		playerLegs.transform.localPosition = playerLegsPrefab.transform.localPosition;
		playerArms.transform.parent = newPlayer.transform;
		playerArms.transform.localPosition = playerArmsPrefab.transform.localPosition;

		newPlayer.name = "player" + Random.Range(1, 20);
	    TeamGameMode mode = GlobalConfig.globalConfig.gamemode as TeamGameMode;
	    if (mode != null) {
	        Team team = newPlayer.GetComponent<Team>();
	        team.team = mode.localTeam;
	        team.enabled = true;
	    }

	    NetworkServer.Spawn(newPlayer);
		NetworkServer.Spawn(playerCam);
		NetworkServer.Spawn(playerLegs);
		NetworkServer.Spawn(playerArms);

		NetworkServer.AddPlayerForConnection(connectionToClient, newPlayer, 1);
		id = newPlayer.GetComponent<Player>().netId;
		camId = playerCam.GetComponent<NetworkIdentity>().netId;
		legsId = playerLegs.GetComponent<NetworkIdentity>().netId;
		playerCam.GetComponent<NetworkParenter>().setParentId(id);
		playerLegs.GetComponent<NetworkParenter>().setParentId(id);
		playerArms.GetComponent<NetworkParenter>().setParentId(id);

	}

	[Server]
	public void destroyPlayer() {
		isDead = true;

		Destroy(player);
	}

	[Client]
	private void setPlayerDead(bool dead) {
		isDead = dead;
		if(isLocalPlayer) {
			switchCamera();
		}
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
		Player playerScript = player.GetComponent<Player>();
		playerScript.controller = this;
		if(!playerScript.isLocalPlayer) {
			cameras.Add(id);
			playerScript.clientController = this;
		}
	}

	[Client]
	private void setCamId(NetworkInstanceId camId) {
		this.camId = camId;
		player = ClientScene.FindLocalObject(id);
		GameObject cam = ClientScene.FindLocalObject(camId);
		if(player.GetComponent<Player>().isLocalPlayer) {
			disableSceneCam();
			cam.GetComponentInChildren<Camera>().enabled = true;
			cam.GetComponentInChildren<AudioListener>().enabled = true;
		}
	}

	[Client]
	private void setLegsId(NetworkInstanceId legsId) {
		this.legsId = legsId;
	}

	[ClientRpc]
	private void RpcSwitchCam() {
		switchCamera();
	}

	[Client]
	private void switchCamera() {
		if(!isDead)
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
		if(!isLocalPlayer)
			return;
		if(sceneCamera != null) {
			sceneCamera.enabled = false;
			sceneCamera.GetComponent<AudioListener>().enabled = false;
		}
	}

	[Client]
	private void enableSceneCam() {
		if(!isLocalPlayer)
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
		if(currentCamera < cameras.Count && currentCamera >= 0) {
			Camera nextCam = ClientScene.FindLocalObject(cameras[currentCamera]).GetComponentInChildren<Camera>();
			if(nextCam != null) {
				nextCam.enabled = true;
				nextCam.GetComponent<AudioListener>().enabled = true;
			}
		} else if(cameras.Count < 1) {
			enableSceneCam();
		}
	}
}
