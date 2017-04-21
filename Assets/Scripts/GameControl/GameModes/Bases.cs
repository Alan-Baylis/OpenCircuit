﻿using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine;

public class Bases : TeamGameMode {

    public float playerRobotPenalty = 1.5f;
    public float respawnDelay = 3f;
	public float scoreCoalescePeriod = 1f;
	public float scoreDisplayPeriod = 5f;

	public CentralRobotController centralRobotControllerPrefab;
	public List<Label> firstTeamLocations = new List<Label>();
	public List<Label> secondTeamLocations = new List<Label>();
	Dictionary<ClientController, float> scoreMap = new Dictionary<ClientController, float>();

	private RobotSpawner[] spawners;

	private List<RespawnJob> respawnJobs = new List<RespawnJob>();

	private Dictionary<int, CentralRobotController> centralRobotControllers = new Dictionary<int, CentralRobotController>();

	private AbstractPlayerSpawner myPlayerSpawner;
	private int remainingRespawnTime;


	//Fields for client side display
	private List<ScoreAdd> scoreUpdates = new List<ScoreAdd>();
	private RespawnJob ? clientRespawnJob;
	private float? clientScore;

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
				HUD.hud.setFireflyElement("respawnTimer", FireflyFont.getString(
					timeLeft.ToString(), 0.02f, new Vector2(-.01f, -.01f), true), false);
				remainingRespawnTime = timeLeft;
			}
			if (remainingRespawnTime == 0) {
				clientRespawnJob = null;
				HUD.hud.clearFireflyElement("respawnTimer");
			}
		}

		if (clientScore != null) {
			Color prevColor = HUD.hud.fireflyConfig.fireflyColor;
			if (clientScore.Value > 100) {
				HUD.hud.fireflyConfig.fireflyColor = Color.blue;
			}
			HUD.hud.setFireflyElement("clientScore",
				FireflyFont.getString(clientScore.Value.ToString("0."), .01f, new Vector2(-.8f, -.3f), true), false);
			HUD.hud.fireflyConfig.fireflyColor = prevColor;

		}

		if (scoreUpdates.Count > 0) {
			for (int i = 0; i < scoreUpdates.Count; ++i) {
				ScoreAdd update = scoreUpdates[i];
				HUD.hud.setFireflyElement("scoreUpdate-" +i ,
					FireflyFont.getString("+"+update.amount.ToString("0."), .01f, new Vector2(-.8f, -.2f + i*0.1f), true), false);
			}
			cleanupScoreDisplay();
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
		return 60*scoreMap[client] / (Time.time - client.startTime);
	}

	[ClientRpc]
	private void RpcUpdateClientScore(NetworkInstanceId localClient, float currentScore, float scoreAdd) {
		if (GlobalConfig.globalConfig.localClient.netId == localClient) {
			clientScore = currentScore;
			addScoreItem(new ScoreAdd(scoreAdd, scoreCoalescePeriod, scoreDisplayPeriod));
		}
	}

	[ClientRpc]
	private void RpcStartTimerFor(NetworkInstanceId localClient) {
		if (GlobalConfig.globalConfig.localClient.netId == localClient) {
			clientRespawnJob = new RespawnJob(GlobalConfig.globalConfig.localClient, respawnDelay);
		}
	}

	private void addScoreItem(ScoreAdd item) {
		if (scoreUpdates.Count > 0) {
			if (scoreUpdates[scoreUpdates.Count-1].coalescePeriod > Time.time) {
				ScoreAdd lastUpdate = scoreUpdates[scoreUpdates.Count-1];
				scoreUpdates[scoreUpdates.Count-1] = new ScoreAdd(lastUpdate.amount + item.amount, scoreCoalescePeriod, scoreDisplayPeriod);
			} else {
				scoreUpdates.Add(item);
			}
		} else {
			scoreUpdates.Add(item);
		}
	}

	private void cleanupScoreDisplay() {
		for (int i = scoreUpdates.Count - 1; i >= 0; --i) {
			ScoreAdd update = scoreUpdates[i];

			if (update.expirationTime < Time.time) {
				HUD.hud.clearFireflyElement("scoreUpdate-"+(scoreUpdates.Count-1));
				scoreUpdates.RemoveAt(i);
			}
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
