using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine;

public class Bases : TeamGameMode {

	public int maxTowers = 3;

    public float playerRobotPenalty = 1.5f;
    public float respawnDelay = 3f;
	public float scoreDisplayPeriod = 5f;

	public CentralRobotController centralRobotControllerPrefab;
	public List<Label> firstTeamLocations = new List<Label>();
	public List<Label> secondTeamLocations = new List<Label>();
	Dictionary<ClientController, ClientInfo> clientInfoMap = new Dictionary<ClientController, ClientInfo>();

	private RobotSpawner[] spawners;

	private List<RespawnJob> respawnJobs = new List<RespawnJob>();

	private Dictionary<int, CentralRobotController> centralRobotControllers = new Dictionary<int, CentralRobotController>();

	private AbstractPlayerSpawner myPlayerSpawner;
	private int remainingRespawnTime;


	//Fields for client side display
	public Dictionary<ClientController, float> clientScoreMap = new Dictionary<ClientController, float>();

	private float scoreAdd;
	private float scoreSubtract;
	private float lastScoreAdd;
	private float lastScoreSubtract;
	private RespawnJob ? clientRespawnJob;
	private float nextScoreUpdate;

	private AbstractPlayerSpawner playerSpawner {
		get {
			if (myPlayerSpawner == null) {
				myPlayerSpawner = FindObjectOfType<AbstractPlayerSpawner>();
			}
			return myPlayerSpawner;
		}
	}

	[ServerCallback]
	public override void Start() {
		base.Start();

		foreach (Label location in firstTeamLocations) {
			getCRC(0).sightingFound(location.labelHandle, location.transform.position, null);
		}

		foreach (Label location in secondTeamLocations) {
			getCRC(1).sightingFound(location.labelHandle, location.transform.position, null);
		}

		spawners = FindObjectsOfType<RobotSpawner>();
		foreach (RobotSpawner spawner in spawners) {
			Label spawnerLabel = spawner.GetComponent<Label>();
			spawnerLabel.setTag(new Tag(TagEnum.Defendable, 0, spawnerLabel.labelHandle));
			getCRC(spawner.teamId.id).sightingFound(spawnerLabel.labelHandle, spawner.transform.position, null);
		}
	}

	[Server]
	public override void onWinGame() {

		HashSet<ClientController> clients = GlobalConfig.globalConfig.clients;
		foreach (ClientController client in clients) {
			GlobalConfig.globalConfig.leaderboard.addScore(new Leaderboard.LeaderboardEntry(
				client.playerName,
				getScore(client)
			));
		}
	}

	protected override void Update() {
		if (isServer) {
			base.Update();
			for (int i = respawnJobs.Count - 1; i >= 0; --i) {
				if (respawnJobs[i].respawnTime <= Time.time) {
					playerSpawner.respawnPlayer(respawnJobs[i].controller);
					respawnJobs.RemoveAt(i);
				}
			}
		}
		if (clientRespawnJob != null) {
			int timeLeft = Mathf.CeilToInt(clientRespawnJob.Value.respawnTime - Time.time);
			if (timeLeft != remainingRespawnTime) {
				HUD.hud.setFireflyElement("respawnTimer", this, FireflyFont.getString(
					timeLeft.ToString(), 0.2f, new Vector2(-.01f, -.01f), FireflyFont.HAlign.CENTER), false);
				remainingRespawnTime = timeLeft;
			}
			if (remainingRespawnTime == 0) {
				clientRespawnJob = null;
				HUD.hud.clearFireflyElement("respawnTimer");
			}
		}

		if (nextScoreUpdate < Time.time && GlobalConfig.globalConfig.localClient != null) {
			showClientScore(GlobalConfig.globalConfig.localClient, false);
			nextScoreUpdate = Time.time + 1;
		}

		showScoreAddition(false);
		showScoreSubtraction(false);
	}

    public override void initialize() {
        base.initialize();
        localTeamId =0;
		if (isServer) {
			initializeCRCs();
		}
	}

	[Server]
	public override bool winConditionMet() {
		foreach (RobotSpawner spawner in spawners) {
			if (spawner == null) {
				continue;
			}
			if (spawner.teamId.id != localTeamId) {
				return false;
			}
		}
		return true;
	}

	[Server]
	public override bool loseConditionMet() {
		foreach (RobotSpawner spawner in spawners) {
			if (spawner == null) {
				continue;
			}
			if (spawner.teamId.id == localTeamId) {
				return false;
			}
		}
		return true;
	}

	[Server]
	public override void onPlayerDeath(Player player) {
		RpcStartTimerFor(player.clientController.netId);
		player.clientController.destroyPlayer();
		respawnJobs.Add(new RespawnJob(player.clientController, respawnDelay));
	}

	[Server]
	public override void onPlayerRevive(Player player) {
		throw new System.NotImplementedException();
	}

	[Server]
	public CentralRobotController getCRC(int teamIndex) {
		return teamIndex < 0 || teamIndex > centralRobotControllers.Count ? null : centralRobotControllers[teamIndex];
	}

	public double getRobotTiming()
	{
		double robotAITime = 0f;
		foreach (CentralRobotController controller in centralRobotControllers.Values) {
			robotAITime += controller.robotExecutionTimer.getMeasuredTimePerSecond();
		}
		return robotAITime;
	}

	public override int getMaxRobots(int teamIndex) {
		return (int)(GlobalConfig.globalConfig.configuration.robotsPerPlayer - getJoinedPlayerCount(teamIndex) *playerRobotPenalty);
	}

	public override int getJoinedPlayerCount(int teamIndex) {
		if (teamIndex == 0) {
			return GlobalConfig.globalConfig.getPlayerCount();
		}
		return 0;
	}

	[Server]
	public void addTower(ClientController owner, GameObject tower) {
		getInfo(owner).towers.Add(tower);
	}

	[Server]
	public bool canBuildTower(ClientController owner) {
		int towerCount = getTowers(owner);
		if (towerCount >= maxTowers)
			return false;
		switch (towerCount) {
			case 0:
				return getScore(owner) > 100;
			case 1:
				return getScore(owner) > 500;
			default:
				return getScore(owner) > 1000;
		}
	}

	[Server]
	private int getTowers(ClientController owner) {
		List<GameObject> towers = getInfo(owner).towers;
		for (int i = towers.Count - 1; i >= 0; --i) {
			if (towers[i] == null) {
				towers.RemoveAt(i);
			}
		}
		return towers.Count;
	}

	[Server]
	public void addScore(ClientController owner, float value) {
		getInfo(owner).score += value;
		RpcUpdateClientScore(owner.netId, getScore(owner), value);
	}

	[Server]
	public void addTeamScore( int teamId, float value) {
		HashSet<ClientController> clients = GlobalConfig.globalConfig.clients;
		foreach (ClientController client in clients) {
			addScore(client, value);
		}
	}

	public float getScore(ClientController client) {
		return adjustScoreForTime(getInfo(client).score, client.startTime);
	}

	public static float adjustScoreForTime(float score, float startTime) {
		return 60 * score / (Time.time - startTime);
	}

	[ClientRpc]
	private void RpcUpdateClientScore(NetworkInstanceId localClient, float currentScore, float scoreAdd) {
		ClientController clientController = ClientScene.FindLocalObject(localClient).GetComponent<ClientController>();
		if (clientController != null) {
			clientScoreMap[clientController] = currentScore;
		}
		if (GlobalConfig.globalConfig.localClient.netId == localClient) {
			addScore(scoreAdd);
			showClientScore(clientController, true);
		}
	}

	[ClientRpc]
	private void RpcStartTimerFor(NetworkInstanceId localClient) {
		if (GlobalConfig.globalConfig.localClient.netId == localClient) {
			clientRespawnJob = new RespawnJob(GlobalConfig.globalConfig.localClient, respawnDelay);
		}
	}

	[Client]
	private void addScore(float value) {
		if (value < 0) {
			scoreSubtract += value;
			lastScoreSubtract = Time.time;
			showScoreSubtraction(true);
		} else {
			scoreAdd += value;
			lastScoreAdd = Time.time;
			showScoreAddition(true);
		}
	}

	private void showClientScore(ClientController client, bool shuffle) {
		if (clientScoreMap.ContainsKey(client)) {
			float score = clientScoreMap[GlobalConfig.globalConfig.localClient];
			Fireflies.Config config = HUD.hud.fireflyConfig;
			config.fireflySize *= 0.25f;
			if (score >= 100)
				config.fireflyColor = new Color(0.25f, 0.25f, 1);
			HUD.hud.setFireflyElementConfig("clientScore", config);

			HUD.hud.setFireflyElement("clientScore", this,
				FireflyFont.getString(adjustScoreForTime(score, client.startTime).ToString("0."), 0.035f,
					new Vector2(0, -0.48f), FireflyFont.HAlign.CENTER), shuffle);
		}
	}

	private void showScoreAddition(bool shuffle) {
		if (scoreAdd <= 0)
			return;
		if (lastScoreAdd < Time.time - scoreDisplayPeriod) {
			scoreAdd = 0;
			HUD.hud.clearFireflyElement("scoreAdd");
		} else {
			Fireflies.Config config = HUD.hud.fireflyConfig;
			config.fireflySize *= 0.5f;
			config.spawnPosition = new Rect(-0.1f, -0.1f, 0.2f, 0.2f);
			config.spawnSpeed = new Rect(-10, -50, 20, 50);
			HUD.hud.setFireflyElementConfig("scoreAdd", config);

			HUD.hud.setFireflyElement("scoreAdd", this,
				FireflyFont.getString("+" + scoreAdd.ToString("0."), 0.06f,
					new Vector2(0, -0.4f), FireflyFont.HAlign.CENTER), shuffle);
		}
	}

	private void showScoreSubtraction(bool shuffle) {
		if (scoreSubtract >= 0)
			return;
		if (lastScoreSubtract < Time.time - scoreDisplayPeriod) {
			scoreSubtract = 0;
			HUD.hud.clearFireflyElement("scoreSubtract");
		} else {
			Fireflies.Config config = HUD.hud.fireflyConfig;
			config.fireflySize *= 0.5f;
			config.fireflyColor = Color.red;
			HUD.hud.setFireflyElementConfig("scoreSubtract", config);

			HUD.hud.setFireflyElement("scoreSubtract", this,
				FireflyFont.getString(scoreSubtract.ToString("0."), 0.05f,
					new Vector2(0, -0.34f), FireflyFont.HAlign.CENTER), shuffle);
		}
	}

	[Server]
	private void initializeCRCs() {
		foreach (Team team in teams.Values) {
			centralRobotControllers.Add(team.id, Instantiate(centralRobotControllerPrefab, Vector3.zero, Quaternion.identity));
		}
	}

	private ClientInfo getInfo(ClientController clientController) {
		ClientInfo info;
		if (!clientInfoMap.TryGetValue(clientController, out info)) {
			info = new ClientInfo();
			clientInfoMap.Add(clientController, info);
		}
		return info;
	}

	private struct RespawnJob {
		public readonly ClientController controller;
		public readonly float respawnTime;

		public RespawnJob(ClientController playerController, float respawnTime) {
			controller = playerController;
			this.respawnTime = Time.time + respawnTime;
		}
	}

	private class ClientInfo {
		public float score, comboScore;
		public int buildPoints;
		public List<GameObject> towers = new List<GameObject>();
	}
}
