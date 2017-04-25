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
	Dictionary<ClientController, float> scoreMap = new Dictionary<ClientController, float>();
	Dictionary<ClientController, List<GameObject>> towerMap  = new Dictionary<ClientController, List<GameObject>>();

	private RobotSpawner[] spawners;

	private List<RespawnJob> respawnJobs = new List<RespawnJob>();

	private Dictionary<int, CentralRobotController> centralRobotControllers = new Dictionary<int, CentralRobotController>();

	private AbstractPlayerSpawner myPlayerSpawner;
	private int remainingRespawnTime;


	//Fields for client side display
	private float scoreAdd;
	private float scoreSubtract;
	private float lastScoreAdd;
	private float lastScoreSubtract;
	private RespawnJob ? clientRespawnJob;
	private float? clientScore;
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

		if (nextScoreUpdate < Time.time) {
			showClientScore(false);
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
		if (!towerMap.ContainsKey(owner)) {
			towerMap[owner] = new List<GameObject>();
		}
		towerMap[owner].Add(tower);
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
		if (!towerMap.ContainsKey(owner))
			return 0;
		List<GameObject> towers = towerMap[owner];
		for (int i = towers.Count - 1; i >= 0; --i) {
			if (towers[i] == null) {
				towers.RemoveAt(i);
			}
		}
		return towers.Count;
	}

	[Server]
	public void addScore(ClientController owner, float value) {
		if (scoreMap.ContainsKey(owner)) {
			scoreMap[owner] += value;
		} else {
			scoreMap[owner] = value;
		}
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
		if (!scoreMap.ContainsKey(client))
			return 0;
		return 60*scoreMap[client] / (Time.time - client.startTime);
	}

	[ClientRpc]
	private void RpcUpdateClientScore(NetworkInstanceId localClient, float currentScore, float scoreAdd) {
		if (GlobalConfig.globalConfig.localClient.netId == localClient) {
			clientScore = currentScore;
			addScore(scoreAdd);
		}
	}

	[ClientRpc]
	private void RpcStartTimerFor(NetworkInstanceId localClient) {
		if (GlobalConfig.globalConfig.localClient.netId == localClient) {
			clientRespawnJob = new RespawnJob(GlobalConfig.globalConfig.localClient, respawnDelay);
		}
	}

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
		showClientScore(true);
	}

	private void showClientScore(bool shuffle) {
		if (clientScore != null) {
			Fireflies.Config config = HUD.hud.fireflyConfig;
			config.fireflySize *= 0.25f;
			if (clientScore.Value >= 100)
				config.fireflyColor = new Color(0.25f, 0.25f, 1);
			HUD.hud.setFireflyElementConfig("clientScore", config);

			HUD.hud.setFireflyElement("clientScore", this,
				FireflyFont.getString(getScore(GlobalConfig.globalConfig.localClient).ToString("0."), 0.035f,
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

	private struct ScoreAdd {
		public readonly float coalescePeriod;
		public readonly float expirationTime;
		public readonly float amount;

		public ScoreAdd(float amount, float coalescePeriod, float expirationPeriod) {
			expirationTime = Time.time + expirationPeriod;
			this.coalescePeriod = Time.time + coalescePeriod;
			this.amount = amount;
		}
	}

	private struct RespawnJob {
		public readonly ClientController controller;
		public readonly float respawnTime;

		public RespawnJob(ClientController playerController, float respawnTime) {
			controller = playerController;
			this.respawnTime = Time.time + respawnTime;
		}
	}
}
