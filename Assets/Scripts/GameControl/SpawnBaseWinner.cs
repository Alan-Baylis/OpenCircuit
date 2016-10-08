using UnityEngine;
using UnityEngine.Networking;

public class SpawnBaseWinner : NetworkBehaviour {

	private RobotSpawner[] spawners;
	private bool hasWon = false;
	
	[ServerCallback]
	public void Start () {
		spawners = GameObject.FindObjectsOfType<RobotSpawner>();
		if (spawners.Length < 1)
			enabled = false;
	}
	
	[ServerCallback]
	public void Update () {
		if (hasWon || spawners.Length < 1)
			return;
		foreach(RobotSpawner spawner in spawners) {
			if (spawner != null)
				return;
		}
		GlobalConfig.globalConfig.winGame();
		hasWon = true;
	}
}
