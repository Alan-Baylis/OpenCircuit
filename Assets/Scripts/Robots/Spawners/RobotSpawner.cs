using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class RobotSpawner : AbstractRobotSpawner {

	protected float lastSpawnTime;

	[ServerCallback]
	public override void Update() {
        base.Update();
		if (active && RobotController.controllerCount < getConfig().getMaxRobots()) {
			if (Time.time -lastSpawnTime > getConfig().getDelay()) {
				spawnRobot();
				lastSpawnTime = Time.time;
			}
		}
	}

}
