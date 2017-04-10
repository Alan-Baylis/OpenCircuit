using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine;

public class Bases : TeamGameMode {

    public float respawnDelay = 3f;

    private RobotSpawner[] spawners;
    private List<RespawnJob> respawnJobs = new List<RespawnJob>();

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

    public override void initialize() {
        localTeam = teams[0];
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

    public override void onPlayerRevive(Player player) {
        throw new System.NotImplementedException();
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
