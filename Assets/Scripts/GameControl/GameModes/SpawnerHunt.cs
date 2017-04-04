using UnityEngine;
using UnityEngine.Networking;

public class SpawnerHunt : GameMode {

    public int frozenPlayers;
    public GameObject freezeLockPrefab;

	private RobotSpawner[] spawners;

	[ServerCallback]
	public void Start() {
	    base.Start();
		spawners = FindObjectsOfType<RobotSpawner>();
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
        return frozenPlayers > 0 && frozenPlayers >= ClientController.numPlayers;
    }

    [Server]
    public override void onPlayerDeath(Player player) {
        GameObject newFreezeLock = Instantiate(freezeLockPrefab);
        newFreezeLock.GetComponent<NetworkParenter>().setParentId(player.netId);
        FreezeLock freezeLockScript = newFreezeLock.AddComponent<FreezeLock>();
        freezeLockScript.frozenPlayer = player;
        NetworkServer.Spawn(newFreezeLock);
        Label label = player.GetComponent<Label>();
        label.setTag(new Tag(TagEnum.Frozen, 0, label.labelHandle));
        player.frozen = true;
        ++frozenPlayers;
    }

    public override void onPlayerRevive(Player player) {
        player.frozen = false;
        player.GetComponent<Label>().clearTag(TagEnum.Frozen);
        --frozenPlayers;
    }
}
