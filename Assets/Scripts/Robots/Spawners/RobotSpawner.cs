using UnityEngine;
using UnityEngine.Networking;

public class RobotSpawner : AbstractRobotSpawner {

	protected float lastSpawnTime;

	[ServerCallback]
	public override void Update() {
        base.Update();
		int teamId = GetComponent<TeamId>().id;
		if (active && Time.time -lastSpawnTime > GlobalConfig.globalConfig.getDelay(teamId)) {
			if (GetComponent<TeamId>().enabled && GlobalConfig.globalConfig.gamemode is TeamGameMode) {
				if (GlobalConfig.globalConfig.teamGameMode.teams[teamId].robotCount >= GlobalConfig.globalConfig.teamGameMode.getMaxRobots(teamId))
					return;
			} else if (GlobalConfig.globalConfig.getRobotCount() >= GlobalConfig.globalConfig.getMaxRobots()) {
				return;
			}
			spawnRobot();
			lastSpawnTime = Time.time;
		}
	}
}
