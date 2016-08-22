using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GlobalConfig : NetworkBehaviour {

	public int robotsPerPlayer = 3;
	public float robotSpawnRatePerSecond = 1f;
	public float spawnRateIncreasePerPlayer = .1f;
	public CentralRobotController centralRobotController;

    public int frozenPlayers = 0;

    [ServerCallback]
    void Update() {
        if (frozenPlayers >= ClientController.numPlayers) {
            RpcLoseGame();
        }
    }

    private static GlobalConfig myGlobalConfig = null;
    public static GlobalConfig globalConfig {
        get {
            if (myGlobalConfig == null)
                myGlobalConfig = GameObject.FindGameObjectWithTag("GlobalConfig").GetComponent<GlobalConfig>();
            return myGlobalConfig;
        }
    }

    [ClientRpc]
    private void RpcLoseGame() {
        Menu.menu.lose();
    }

	[Server]
	public int getMaxRobots() {
		//TODO: I imagine this will have to change to support spectators -Brian
		return NetworkServer.connections.Count * robotsPerPlayer;
	}

	[Server]
	public float getDelay() {
		return 1f/(NetworkServer.connections.Count * spawnRateIncreasePerPlayer + robotSpawnRatePerSecond); 
	}

	[Server]
	public CentralRobotController getCRC() {
		return centralRobotController;
	}
}
