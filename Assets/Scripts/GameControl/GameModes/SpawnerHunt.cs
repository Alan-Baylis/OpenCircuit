using UnityEngine;
using UnityEngine.Networking;

public class SpawnerHunt : GameMode {

	private RobotSpawner[] spawners;

	[ServerCallback]
	public void Start() {
	    base.Start();
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

    [Server]
    public override bool loseConditionMet() {
        return GlobalConfig.globalConfig.frozenPlayers > 0 && GlobalConfig.globalConfig.frozenPlayers >= ClientController.numPlayers;
    }
}
