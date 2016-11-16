using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GlobalConfig : NetworkBehaviour {

	[SyncVar]
	public GlobalConfigData configuration = GlobalConfigData.getDefault();
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
            return myGlobalConfig;
        }
    }

	public GlobalConfig() {
		myGlobalConfig = this;
	}

    [Server]
    public void winGame() {
        RpcWinGame();
    }

    [Server]
    public void loseGame() {
        RpcLoseGame();
    }

    [ClientRpc]
    private void RpcLoseGame() {
        Menu.menu.lose();
    }

    [ClientRpc]
    private void RpcWinGame() {
        Menu.menu.win();
    }

	[Server]
	public int getMaxRobots() {
		//TODO: I imagine this will have to change to support spectators -Brian
		return NetworkServer.connections.Count * configuration.robotsPerPlayer;
	}

	[Server]
	public float getDelay() {
		return 1f/(NetworkServer.connections.Count * configuration.spawnRateIncreasePerPlayer + configuration.robotSpawnRatePerSecond); 
	}

	[Server]
	public CentralRobotController getCRC() {
		return centralRobotController;
	}
}

[System.Serializable]
public struct GlobalConfigData {
	public int robotsPerPlayer;
    public float robotSpawnRatePerSecond;
	public float spawnRateIncreasePerPlayer;

	public static GlobalConfigData getDefault() {
		GlobalConfigData data = new GlobalConfigData();
		data.robotsPerPlayer = 3;
		data.robotSpawnRatePerSecond = 1f;
		data.spawnRateIncreasePerPlayer = 0.1f;
		return data;
	}
}