using UnityEngine;
using UnityEngine.Networking;

public class RobotSpawner : AbstractRobotSpawner {

	protected float lastSpawnTime;

	[ServerCallback]
	public override void Update() {
        base.Update();
		if (active && GlobalConfig.globalConfig != null && GlobalConfig.globalConfig.robotControllers < GlobalConfig.globalConfig.getMaxRobots()) {
			if (Time.time -lastSpawnTime > GlobalConfig.globalConfig.getDelay()) {
				spawnRobot();
				lastSpawnTime = Time.time;
			}
		}
	}
}
