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
        if (GlobalConfig.globalConfig.frozenPlayers > 0 && GlobalConfig.globalConfig.frozenPlayers >=
            ClientController.numPlayers)
            return true;
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
}
