using UnityEngine.Networking;

public class Bases : TeamGameMode {

    private RobotSpawner[] spawners;

    [ServerCallback]
    public void Start() {
        base.Start();
        spawners = FindObjectsOfType<RobotSpawner>();
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

    public override void onPlayerDeath(Player player) {
        throw new System.NotImplementedException();
    }

    public override void onPlayerRevive(Player player) {
        throw new System.NotImplementedException();
    }
}
