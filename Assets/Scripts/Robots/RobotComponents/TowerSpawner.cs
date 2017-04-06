using UnityEngine;
using UnityEngine.Networking;

public class TowerSpawner : AbstractRobotComponent {

	public GameObject towerPrefab;
	public GameObject towerTurretPrefab;
	public GameObject eyesPrefab;
	public GameObject generatorPrefab;
	public Vector3 offset;


	[Server]
	public GameObject buildTower(Vector3 position) {
		GameObject newTower = Instantiate(towerPrefab, position + offset, towerPrefab.transform.rotation);
		GameObject towerTurret = Instantiate(towerTurretPrefab, position+offset+towerTurretPrefab.transform.position,
			towerTurretPrefab.transform.rotation, newTower.transform);
		GameObject eyes = Instantiate(eyesPrefab, position+offset+eyesPrefab.transform.position,
			eyesPrefab.transform.rotation, newTower.transform);
		GameObject gen = Instantiate(generatorPrefab, position+offset+generatorPrefab.transform.position,
			generatorPrefab.transform.rotation, newTower.transform);

		RobotController controller = newTower.GetComponent<RobotController>();
		towerTurret.GetComponent<NetworkParenter>().setParentId(controller.netId);
		eyes.GetComponent<NetworkParenter>().setParentId(controller.netId);
		gen.GetComponent<NetworkParenter>().setParentId(controller.netId);

		NetworkServer.Spawn(newTower);
		NetworkServer.Spawn(towerTurret);
		NetworkServer.Spawn(eyes);
		NetworkServer.Spawn(gen);

		if (getController().GetComponent<Team>().enabled && GlobalConfig.globalConfig.gamemode is Bases) {
			Bases gameMode = ((Bases)GlobalConfig.globalConfig.gamemode);
			int teamId = getController().GetComponent<Team>().team.Id;
			gameMode.getCRC(teamId).forceAddListener(controller);
			Team team = newTower.GetComponent<Team>();
			team.team = gameMode.teams[teamId];
			team.enabled = true;
		}
		return newTower;
	}
}
