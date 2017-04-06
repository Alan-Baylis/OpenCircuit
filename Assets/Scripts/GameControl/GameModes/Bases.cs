using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine;

public class Bases : TeamGameMode {

    public float respawnDelay = 3f;
	public CentralRobotController centralRobotControllerPrefab;

	private RobotSpawner[] spawners;
	private List<RespawnJob> respawnJobs = new List<RespawnJob>();

	private List<CentralRobotController> centralRobotControllers = new List<CentralRobotController>();

	[ServerCallback]
    public void Start() {
        base.Start();
        spawners = FindObjectsOfType<RobotSpawner>();
    }

    [ServerCallback]
    protected override void Update() {
        base.Update();
        for (int i = 0; i < respawnJobs.Count; ++i) {
            if (respawnJobs[i].timeRemaining <= 0f) {
                FindObjectOfType<AbstractPlayerSpawner>().respawnPlayer(respawnJobs[i].controller);
            } else {
                respawnJobs[i] = new RespawnJob(respawnJobs[i].controller, respawnJobs[i].timeRemaining - Time.deltaTime);
            }
        }
    }

	[Server]
    public override void initialize() {
        localTeam = teams[0];
		initializeCRCs();
    }

    [Server]
	public override bool winConditionMet() {
        foreach (RobotSpawner spawner in spawners) {
            if (spawner == null) {
                continue;
            }
            if (spawner.team.Id != localTeam.Id) {
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
            if (spawner.team.Id == localTeam.Id) {
                return false;
            }
        }
        return true;
    }

    [Server]
    public override void onPlayerDeath(Player player) {
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

	[Server]
	private void initializeCRCs() {
		foreach (TeamData teamData in teams) {
			centralRobotControllers.Add(Instantiate(centralRobotControllerPrefab, Vector3.zero, Quaternion.identity));
		}
	}

    private struct RespawnJob {
        public ClientController controller;
        public float timeRemaining;

        public RespawnJob(ClientController playerController, float time) {
            controller = playerController;
            timeRemaining = time;
        }
    }
}
