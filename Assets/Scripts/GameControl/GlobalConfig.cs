using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GlobalConfig : NetworkBehaviour {

	public int robotsPerPlayer = 3;
	public float robotSpawnRatePerSecond = 1f;
	public float spawnRateIncreasePerPlayer = .1f;
	public int maxRobots = 0;
	public float delay = 0f;

	[ServerCallback]
	void Update() {
		maxRobots = getMaxRobots();
		delay = getDelay();
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
}
