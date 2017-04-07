using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class ClientController : NetworkBehaviour {

	private static List<CamInfo> availableCameras = new List<CamInfo>();
	private static Dictionary<ClientController, CamInfo> cameraMap = new Dictionary<ClientController, CamInfo>();

	public GameObject playerPrefab;
	public GameObject playerCamPrefab;
	public GameObject playerLegsPrefab;
	public GameObject playerArmsPrefab;
    public static int numPlayers = 0;

	[SyncVar(hook="setPlayerId")]
	private NetworkInstanceId id;
	[SyncVar(hook="setCamId")]
	private NetworkInstanceId camId;
	private GameObject player;

	[SyncVar(hook="setPlayerDead")]
	private bool isDead;

	private static int nextCameraIndex;
	private static CamInfo sceneCamera;
	private static CamInfo lastCamera;
	private static bool localPlayerDead;

	[ClientCallback]
	void Start() {
		if (isLocalPlayer) {
			Camera[] cams = FindObjectsOfType<Camera>();
			foreach (Camera cam in cams) {
				if (cam.tag == "SceneCamera") {
					sceneCamera = new CamInfo(this, cam);
					lastCamera = sceneCamera;
					availableCameras.Add(sceneCamera);
					break;
				}
			}
		}

		if (player != null) {
			addCamera(this, player.GetComponentInChildren<Camera>());
		}

	//	disableSceneCam();
		if(isLocalPlayer) {
			CmdSpawnPlayerAt(transform.position);
		}
	}

	[ClientCallback]
	void Update() {
		if(isLocalPlayer && isDead && Input.GetButtonDown("Use")) {
			switchCamera();
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
		id = newPlayer.GetComponent<Player>().netId;
		playerCam.GetComponent<NetworkParenter>().setParentId(id);
		playerLegs.GetComponent<NetworkParenter>().setParentId(id);
		playerArms.GetComponent<NetworkParenter>().setParentId(id);

		NetworkServer.Spawn(playerCam);
		camId = playerCam.GetComponent<NetworkIdentity>().netId;

		NetworkServer.Spawn(playerLegs);
		NetworkServer.Spawn(playerArms);

		NetworkServer.AddPlayerForConnection(connectionToClient, newPlayer, 1);
	}

	[Server]
	public void destroyPlayer() {
		isDead = true;
		Destroy(player);
	}

	[Client]
	private void setPlayerDead(bool dead) {
		isDead = dead;
		if (isLocalPlayer) {
			localPlayerDead = isDead;
		}
		if (isDead) {
			removeCamera(this);
			if(isLocalPlayer) {
				switchCamera();
			}
		}
	}

	[Server]
	public void respawnPlayerAt(Vector3 position) {
		if(isDead) {
			isDead = false;
			spawnPlayerAt(position);
		}
	}

	[Client]
	private void setPlayerId(NetworkInstanceId id) {
		this.id = id;
		player = ClientScene.FindLocalObject(id);
		Player playerScript = player.GetComponent<Player>();
		playerScript.controller = this;
		if(!playerScript.isLocalPlayer) {
			playerScript.clientController = this;
		}
	}

	[Client]
	private void setCamId(NetworkInstanceId camId) {
		this.camId = camId;
		player = ClientScene.FindLocalObject(id);
		GameObject camObject = ClientScene.FindLocalObject(camId);
		Camera cam = camObject.GetComponentInChildren<Camera>();
		addCamera(this, cam);
		if(isLocalPlayer) {
			if (lastCamera != null) {
				disableCamera(lastCamera.cam);
			}
			enableCamera(cam);
			nextCameraIndex = 0;
		}
	} 

	[Client]
	private void addCamera(ClientController controller, Camera cam) {
		CamInfo newCamInfo = new CamInfo(controller, cam);
		cameraMap[controller] = newCamInfo;
		availableCameras.Add(newCamInfo);
	}

	[Client]
	private static void removeCamera(ClientController controller) {
		availableCameras.Remove(cameraMap[controller]);
		cameraMap.Remove(controller);
		if (localPlayerDead && lastCamera.controller == controller) {
			nextCameraIndex = 0;
			switchCamera();
		}
	}

	[Client]
	private static void switchCamera() {
		if (lastCamera != null)
			disableCamera(lastCamera.cam);
		lastCamera = availableCameras[nextCameraIndex];
		enableCamera(lastCamera.cam);

		incrementCamera();
	}

	private static void enableCamera(Camera cam) {
		cam.enabled = true;
		cam.GetComponent<AudioListener>().enabled = true;
	}

	private static void disableCamera(Camera cam) {
		cam.enabled = false;
		cam.GetComponent<AudioListener>().enabled = false;
	}

	[Client]
	private static void incrementCamera() {
		if(nextCameraIndex >= availableCameras.Count - 1) {
			nextCameraIndex = 0;
		} else {
			nextCameraIndex++;
		}
	}

	private class CamInfo {
		public readonly ClientController controller;
		public readonly Camera cam;

		public CamInfo(ClientController controller, Camera cam) {
			this.controller = controller;
			this.cam = cam;
		}

	}
}