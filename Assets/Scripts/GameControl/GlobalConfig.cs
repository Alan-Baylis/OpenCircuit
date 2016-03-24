using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GlobalConfig : NetworkBehaviour {

	public int robotsPerPlayer = 3;
	public int maxRobots = 0;

	[ServerCallback]
	void Update() {
		maxRobots = NetworkServer.connections.Count * robotsPerPlayer;
	}

	public int getMaxRobots() {
		//TODO: I imagine this will have to change to support spectators -Brian
		return NetworkServer.connections.Count * robotsPerPlayer;
	}
}
