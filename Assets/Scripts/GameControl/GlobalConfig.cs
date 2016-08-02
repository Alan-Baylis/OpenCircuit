using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GlobalConfig : NetworkBehaviour {

	public int robotsPerPlayer = 3;
	public float robotSpawnRatePerSecond = 1f;
	public float spawnRateIncreasePerPlayer = .1f;
	public CentralRobotController centralRobotController;

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
