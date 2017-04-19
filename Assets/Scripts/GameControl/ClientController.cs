using UnityEngine;
using UnityEngine.Networking;

public class ClientController : NetworkBehaviour {

	public GameObject playerPrefab;
	public GameObject playerCamPrefab;
	public GameObject playerLegsPrefab;
	public GameObject playerArmsPrefab;

	[SyncVar]
	public bool spectator;

	[SyncVar(hook="setPlayerId")]
	private NetworkInstanceId id;
	[SyncVar(hook="setCamId")]
	private NetworkInstanceId camId;
	private GameObject player;

	[SyncVar(hook="setPlayerDead")]
	private bool isDead;

	void Start() {
		GlobalConfig.globalConfig.clients.Add(this);
		if (player != null) {
			GlobalConfig.globalConfig.cameraManager.addCamera(this, player.GetComponentInChildren<Camera>());
		}
		if (isLocalPlayer)
			GlobalConfig.globalConfig.localClient = this;

		if(isServer && !spectator) {
			AbstractPlayerSpawner spawner = FindObjectOfType<AbstractPlayerSpawner>();
			if (spawner != null) {
				spawnPlayerAt(spawner.nextSpawnPos());
			} else {
				Debug.LogError("FAILED TO SPAWN PLAYER!!! NO PLAYER SPAWNER EXISTS!!!");
			}
		}
	}

	[ClientCallback]
	void Update() {
		if (isLocalPlayer && (isDead || spectator) && Input.GetButtonDown("Use")) {
			GlobalConfig.globalConfig.cameraManager.switchCamera();
		}
	}

	[ClientCallback]
	public void OnDestroy() {
		if (!isDead && !spectator) {
			GlobalConfig.globalConfig.cameraManager.removeCamera(this);
		}
		if (isLocalPlayer) {
			GlobalConfig.globalConfig.cameraManager.useSceneCamera();
		}
		GlobalConfig.globalConfig.clients.Remove(this);
	}

	//anyone can call!!
	public bool isAlive() {
		return !isDead;
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
	        TeamId team = newPlayer.GetComponent<TeamId>();
	        team.id = mode.localTeamId;
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
		if (isDead) {
			GlobalConfig.globalConfig.cameraManager.removeCamera(this);
			if(isLocalPlayer) {
				GlobalConfig.globalConfig.cameraManager.switchCamera();
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
		GlobalConfig.globalConfig.cameraManager.addCamera(this, cam);
		if (isLocalPlayer) {
			GlobalConfig.globalConfig.cameraManager.usePlayerCam(cam);
		}
	}


}