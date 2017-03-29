using UnityEngine;
using UnityEngine.Networking;

public class SpawnerHunt : GameMode {

	private RobotSpawner[] spawners;

	[ServerCallback]
	public void Start() {
		spawners = GameObject.FindObjectsOfType<RobotSpawner>();
	}

	[Server]
	public override bool winConditionMet() {
		if (spawners.Length < 1)
			return true;
		foreach (RobotSpawner spawner in spawners) {
			if (spawner != null)
				return false;
		}
		return true;
	}

    public override bool loseConditionMet() {
        return GlobalConfig.globalConfig.frozenPlayers >= ClientController.numPlayers;
    }
}
