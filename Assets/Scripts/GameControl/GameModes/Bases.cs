using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine;

public class Bases : TeamGameMode {

    public float playerRobotPenalty = 1.5f;
    public float respawnDelay = 3f;
	public CentralRobotController centralRobotControllerPrefab;

	private RobotSpawner[] spawners;

	private List<RespawnJob> respawnJobs = new List<RespawnJob>();

	private Dictionary<int, CentralRobotController> centralRobotControllers = new Dictionary<int, CentralRobotController>();

	private AbstractPlayerSpawner myPlayerSpawner;
	private int remainingRespawnTime;

	private RespawnJob ? clientRespawnJob;

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
					timeLeft.ToString(), 0.02f, new Vector2(0, -0.4f), true), false);
				remainingRespawnTime = timeLeft;
			}
			if (remainingRespawnTime == 0) {
				clientRespawnJob = null;
				HUD.hud.clearFireflyElement("respawnTimer");
			}
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
		foreach (CentralRobotController controller in centralRobotControllers.Values)
		{
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

	[ClientRpc]
	private void RpcStartTimerFor(NetworkInstanceId localClient) {
		if (GlobalConfig.globalConfig.localClient.netId == localClient) {
			clientRespawnJob = new RespawnJob(GlobalConfig.globalConfig.localClient, respawnDelay);
		}
	}

	[Server]
	private void initializeCRCs() {
		foreach (Team team in teams.Values) {
			centralRobotControllers.Add(team.id, Instantiate(centralRobotControllerPrefab, Vector3.zero, Quaternion.identity));
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
