using UnityEngine;
using UnityEngine.Networking;

public class RobotSpawner : AbstractRobotSpawner {

	protected float lastSpawnTime;

	[ServerCallback]
	public override void Update() {
        base.Update();
		if (active && Time.time -lastSpawnTime > GlobalConfig.globalConfig.getDelay()) {
			int teamId = GetComponent<TeamId>().id;
			if (GetComponent<TeamId>().enabled && GlobalConfig.globalConfig.gamemode is TeamGameMode &&
					GlobalConfig.globalConfig.teamGameMode.teams[teamId].robotCount >= GlobalConfig.globalConfig.teamGameMode.getMaxRobots(teamId)) {
				return;
			}
			spawnRobot();
			lastSpawnTime = Time.time;
		}
	}
}
