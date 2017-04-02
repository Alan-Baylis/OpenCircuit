using UnityEngine;
using UnityEngine.Networking;

public class RobotSpawner : AbstractRobotSpawner {

	protected float lastSpawnTime;

	[ServerCallback]
	public override void Update() {
        base.Update();
		if (active && GlobalConfig.globalConfig != null && RobotController.controllerCount < GlobalConfig.globalConfig.getMaxRobots()) {
			if (Time.time -lastSpawnTime > GlobalConfig.globalConfig.getDelay()) {
				spawnRobot();
				lastSpawnTime = Time.time;
			}
		}
	}

}
