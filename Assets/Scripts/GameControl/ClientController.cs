using UnityEngine;
using UnityEngine.Networking;

public class ClientController : NetworkBehaviour {

	public GameObject spectatorPrefab;

	public GameObject playerPrefab;
	public GameObject playerCamPrefab;
	public GameObject playerLegsPrefab;
	public GameObject playerArmsPrefab;

	[SyncVar] public NetworkController.ClientType clientType;

	private GameObject player;

	[SyncVar]
	private bool isDead;

	[SyncVar]
	public string playerName;

	[SyncVar]
	public int team;

	public float startTime;

	void Start() {
		startTime = Time.time;
		GlobalConfig.globalConfig.clients.Add(this);
		if (player != null) {
			GlobalConfig.globalConfig.cameraManager.addCamera(this, player.GetComponentInChildren<Camera>());
		}
		if (isLocalPlayer)
			GlobalConfig.globalConfig.localClient = this;

		if(isServer) {
			if (isSpectator()) {
				spawnSpectator();
			} else {
				spawnPlayerAt(GlobalConfig.globalConfig.gamemode.getPlayerSpawner(this).nextSpawnPos());
			}
		}
	}

	[ClientCallback]
	void Update() {
		if (isLocalPlayer && isDead && Input.GetButtonDown("Use")) {
			GlobalConfig.globalConfig.cameraManager.switchCamera();
		}
	}

	[ClientCallback]
	public void OnDestroy() {
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
	private void spawnSpectator() {
		GameObject newSpectator = Instantiate(spectatorPrefab);
		newSpectator.GetComponent<Spectator>().clientControllerId = netId;
		NetworkServer.Spawn(newSpectator);
		NetworkServer.AddPlayerForConnection(connectionToClient, newSpectator, 1);
	}
	
	[Server]
	private void spawnPlayerAt(Vector3 position) {
		playerPrefab.SetActive(false);
		GameObject newPlayer = Instantiate(playerPrefab, position, Quaternion.identity);
		GameObject playerCam = Instantiate(playerCamPrefab, position, Quaternion.identity);
		GameObject playerLegs = Instantiate(playerLegsPrefab, position, Quaternion.identity);
		GameObject playerArms = Instantiate(playerArmsPrefab, position, Quaternion.identity);

		Player playerScript = newPlayer.GetComponent<Player>();
		playerScript.clientControllerId = netId;

		newPlayer.SetActive(true);

		playerCam.transform.parent = newPlayer.transform;
		playerCam.transform.localPosition = playerCamPrefab.transform.localPosition;
		playerLegs.transform.parent = newPlayer.transform;
		playerLegs.transform.localPosition = playerLegsPrefab.transform.localPosition;
		playerArms.transform.parent = newPlayer.transform;
		playerArms.transform.localPosition = playerArmsPrefab.transform.localPosition;

		newPlayer.name = "player-" + playerName;
	    TeamGameMode mode = GlobalConfig.globalConfig.gamemode as TeamGameMode;
	    if (mode != null) {
	        TeamId teamId = newPlayer.GetComponent<TeamId>();
		    teamId.id = team;
		    teamId.enabled = true;
	    }

	    NetworkServer.Spawn(newPlayer);
		NetworkInstanceId playerId = newPlayer.GetComponent<Player>().netId;
		newPlayer.GetComponent<NameTag>().displayName = playerName;
		newPlayer.GetComponent<Score>().owner = this;
		playerCam.GetComponent<NetworkParenter>().setParentId(playerId);
		playerLegs.GetComponent<NetworkParenter>().setParentId(playerId);
		playerArms.GetComponent<NetworkParenter>().setParentId(playerId);

		NetworkServer.Spawn(playerCam);
		NetworkServer.Spawn(playerLegs);
		NetworkServer.Spawn(playerArms);

		NetworkServer.AddPlayerForConnection(connectionToClient, newPlayer, 1);
	}

	[Server]
	public void destroyPlayer() {
		isDead = true;
		player.GetComponent<Player>().dismantle();
	}

	[Client]
	public void setPlayer(GameObject player) {
		this.player = player;
	}

	[Server]
	public void respawnPlayerAt(Vector3 position) {
		if(isDead) {
			isDead = false;
			spawnPlayerAt(position);
		}
	}

	public bool isSpectator() {
		return clientType == NetworkController.ClientType.SPECTATOR || clientType == NetworkController.ClientType.ADMIN;
	}
}