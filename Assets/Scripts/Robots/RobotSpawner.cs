using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class RobotSpawner : AbstractRobotSpawner {

	protected float lastSpawnTime;

	[ServerCallback]
	void Update() {
		if (active && RobotController.controllerCount < getConfig().getMaxRobots()) {
			if (Time.time -lastSpawnTime > getConfig().getDelay()) {
				spawnRobot();
				lastSpawnTime = Time.time;
			}
		}
	}

}
