using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine;

public class Bases : TeamGameMode {

	public int maxTowers = 3;

    public float playerRobotPenalty = 1.5f;
    public float respawnDelay = 3f;
	public float scoreDisplayPeriod = 5f;
	public float buildPointDisplayPeriod = 6f;
	public float comboDeteriorationRate = 20;

	public float[] comboScorePerBuildPoint;

	public CentralRobotController centralRobotControllerPrefab;

	public AbstractPlayerSpawner primarySpawner;
	public AbstractPlayerSpawner tutorialSpawner;

	public List<Label> firstTeamLocations = new List<Label>();
	public List<Label> secondTeamLocations = new List<Label>();
	public Dictionary<ClientController, ClientInfo> clientInfoMap = new Dictionary<ClientController, ClientInfo>();

	private RobotSpawner[] spawners;

	private List<RespawnJob> respawnJobs = new List<RespawnJob>();

	private Dictionary<int, CentralRobotController> centralRobotControllers = new Dictionary<int, CentralRobotController>();

	private AbstractPlayerSpawner myPlayerSpawner;
	private int remainingRespawnTime;


	//Fields for client side display
	private float scoreAdd;
	private float scoreSubtract;
	private float buildPointAdd;
	private float lastScoreAdd;
	private float lastScoreSubtract;
	private float lastBuildPointAdd;
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

	public override AbstractPlayerSpawner getPlayerSpawner(ClientController controller) {
		switch (controller.clientType) {
			case NetworkController.ClientType.PLAYER:
				return primarySpawner;
			case NetworkController.ClientType.TUTORIAL:
				return tutorialSpawner;
			default:
				return primarySpawner;
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
			foreach (KeyValuePair<ClientController, ClientInfo> info in clientInfoMap) {
				updateComboScore(info.Value);
				if (info.Value.updateTowerCount())
					RpcUpdateClientScore(info.Key.netId, info.Value.score, 0, 0);
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

		ClientController localClient = GlobalConfig.globalConfig.localClient;
		if (localClient != null) {
			if (nextScoreUpdate < Time.time && localClient != null) {
				showClientScore(localClient, false);
				nextScoreUpdate = Time.time + 1;
			}
			ClientInfo localInfo = getInfo(localClient);
			if (!isServer)
				updateComboScore(localInfo);
			showClientCombo(localClient, false);
			showClientBuildPoints(localClient, false);

			showScoreAddition(false);
			showScoreSubtraction(false);
			showBuildPointAddition(false);
		}
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
	public void addTower(ClientController owner, GameObject tower, GameObject towerBase) {
		getInfo(owner).addTower(tower, towerBase);
	}

	[Server]
	public void spendBuildPoint(ClientController owner, GameObject towerBase) {
		getInfo(owner).addTowerBase(towerBase);
		RpcUpdateClientScore(owner.netId, getInfo(owner).score, 0, 0);
	}

	[Server]
	public bool canBuildTower(ClientController owner) {
		return getInfo(owner).score.buildPoints > 0;
	}

	[Server]
	public void addScore(ClientController owner, float scoreAdd) {
		ClientInfo info = getInfo(owner);
		info.score.total += scoreAdd;
		info.score.combo += scoreAdd;
		int buildPointsAdd = 0;
		while (comboScorePerBuildPoint.Length > info.getCombinedBuildPoints() &&
		       info.score.combo >= comboScorePerBuildPoint[info.getCombinedBuildPoints()]) {
			++info.score.buildPoints;
			++buildPointsAdd;
		}

		RpcUpdateClientScore(owner.netId, info.score, scoreAdd, buildPointsAdd);
	}

	[Server]
	public void addTeamScore(int teamId, float value) {
		HashSet<ClientController> clients = GlobalConfig.globalConfig.clients;
		foreach (ClientController client in clients) {
			addScore(client, value);
		}
	}

	public float getScore(ClientController client) {
		return adjustScoreForTime(getInfo(client).score.total, client.startTime);
	}

	public static float adjustScoreForTime(float score, float startTime) {
		return 60 * score / (Time.time - startTime + 60);
	}

	[ClientRpc]
	private void RpcUpdateClientScore(NetworkInstanceId client, ClientScore score, float scoreAdd, int buildPointAdd) {
		ClientController clientController = ClientScene.FindLocalObject(client).GetComponent<ClientController>();
		if (clientController != null) {
			getInfo(clientController).score = score;
		}
		if (GlobalConfig.globalConfig.localClient.netId == client) {
			addScore(scoreAdd);
			addBuildPoint(buildPointAdd);
			showClientScore(clientController, true);
			showClientCombo(clientController, true);
			showClientBuildPoints(clientController, true);
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

	[Client]
	private void addBuildPoint(int value) {
		if (value > 0) {
			buildPointAdd += value;
			lastBuildPointAdd = Time.time;
			showBuildPointAddition(true);
		}
	}

	private void updateComboScore(ClientInfo info) {
		info.score.combo = Mathf.Max(0, info.score.combo - comboDeteriorationRate * Time.deltaTime);
	}

	[Client]
	private void showClientScore(ClientController client, bool shuffle) {
		float score = adjustScoreForTime(getInfo(client).score.total, client.startTime);
		Fireflies.Config config = HUD.hud.fireflyConfig;
		config.fireflySize *= 0.25f;
		if (score >= 100)
			config.fireflyColor = new Color(0.25f, 0.25f, 1);
		HUD.hud.setFireflyElementConfig("clientScore", config);

		HUD.hud.setFireflyElement("clientScore", this,
			FireflyFont.getString(score.ToString("0."), 0.035f,
				new Vector2(0, -0.48f), FireflyFont.HAlign.CENTER), shuffle);
	}

	[Client]
	private void showClientCombo(ClientController client, bool shuffle) {
		ClientInfo info = getInfo(client);
		if (info.getCombinedBuildPoints() >= comboScorePerBuildPoint.Length) {
			HUD.hud.clearFireflyElement("clientComboScore");
			return;
		}

		float percent = info.score.combo /comboScorePerBuildPoint[info.getCombinedBuildPoints()];
		Fireflies.Config config = HUD.hud.fireflyConfig;
		config.fireflyColor = new Color(0.5f +0.5f *percent, 0.5f +0.5f *percent, 1);
		config.spawnPosition = new Rect(-0.1f, -0.1f, 0.2f, 0.2f);
		HUD.hud.setFireflyElementConfig("clientComboScore", config);

		List<Vector2> positions = new List<Vector2>();
		int totalCount = 20;
		int count = (int)(totalCount *percent);
		for (int i=0; i<count; ++i) {
			positions.Add(new Vector2(-0.02f *(totalCount -i), 0.4f));
			positions.Add(new Vector2(0.02f *(totalCount -i), 0.4f));
		}
		HUD.hud.setFireflyElement("clientComboScore", this, positions, shuffle);
	}

	[Client]
	private void showClientBuildPoints(ClientController client, bool shuffle) {
		ClientInfo info = getInfo(client);
		if (info.score.buildPoints == 0) {
			HUD.hud.clearFireflyElement("clientBuildPoints");
			return;
		}

		float buildPoints = getInfo(client).score.buildPoints;
		Fireflies.Config config = HUD.hud.fireflyConfig;
		config.fireflySize *= 0.25f;
		HUD.hud.setFireflyElementConfig("clientBuildPoints", config);

		HUD.hud.setFireflyElement("clientBuildPoints", this,
			FireflyFont.getString(buildPoints.ToString("0.") + " build_point\npress e to use", 0.04f,
				new Vector2(-0.5f, -0.48f)), shuffle);
	}

	[Client]
	private void showScoreAddition(bool shuffle) {
		if (scoreAdd <= 0 || lastBuildPointAdd >= Time.time - buildPointDisplayPeriod)
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

	[Client]
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
					new Vector2(0, -0.3f), FireflyFont.HAlign.CENTER), shuffle);
		}
	}

	[Client]
	private void showBuildPointAddition(bool shuffle) {
		if (buildPointAdd <= 0)
			return;
		if (lastBuildPointAdd < Time.time - buildPointDisplayPeriod) {
			buildPointAdd = 0;
			HUD.hud.clearFireflyElement("scoreAdd");
		} else {
			Fireflies.Config config = HUD.hud.fireflyConfig;
			config.fireflySize *= 0.5f;
			config.spawnPosition = new Rect(-0.4f, -0.4f, 0.8f, 0.8f);
			config.spawnSpeed = new Rect(-25, -50, 50, 75);
			HUD.hud.setFireflyElementConfig("scoreAdd", config);

			HUD.hud.setFireflyElement("scoreAdd", this,
				FireflyFont.getString("+" + buildPointAdd.ToString("0.") +" build_point", 0.06f,
					new Vector2(0, -0.4f), FireflyFont.HAlign.CENTER), shuffle);
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

	public class ClientInfo {
		public ClientScore score;
		private List<GameObject> towers = new List<GameObject>();

		public int getCombinedBuildPoints() {
			return score.buildPoints + score.towerCount;
		}

		[Server]
		public void addTowerBase(GameObject towerBase) {
			towers.Add(towerBase);
			--score.buildPoints;
			score.towerCount = towers.Count;
		}

		[Server]
		public void addTower(GameObject tower, GameObject towerBase) {
			towers.Remove(towerBase);
			towers.Add(tower);
		}

		[Server]
		public bool updateTowerCount() {
			bool removed = false;
			for (int i = towers.Count - 1; i >= 0; --i) {
				if (towers[i] == null) {
					towers.RemoveAt(i);
					removed = true;
				}
			}
			score.towerCount = towers.Count;
			return removed;
		}
	}

	[System.Serializable]
	public struct ClientScore {
		public float total, combo;
		public int buildPoints, towerCount;
	}
}
