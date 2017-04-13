using UnityEngine;
using UnityEngine.Networking;

public class TowerSpawner : AbstractRobotComponent {

	public GameObject towerPrefab;
	public GameObject[] components;
	public Vector3 offset;


	[Server]
	public GameObject buildTower(Vector3 position) {
		GameObject newTower = Instantiate(towerPrefab, position + offset, towerPrefab.transform.rotation);

		RobotController controller = newTower.GetComponent<RobotController>();
		NetworkServer.Spawn(newTower);

		foreach (GameObject componentPrefab in components) {
			GameObject component = Instantiate(componentPrefab, position + offset + componentPrefab.transform.position, componentPrefab.transform.rotation);
			component.GetComponent<NetworkParenter>().setParentId(controller.netId);
			NetworkServer.Spawn(component);

		}

		if (getController().GetComponent<Team>().enabled && GlobalConfig.globalConfig.gamemode is Bases) {
			Bases gameMode = (Bases)GlobalConfig.globalConfig.gamemode;
			int teamId = getController().GetComponent<Team>().team.Id;
			gameMode.getCRC(teamId).forceAddListener(controller);
			Team team = newTower.GetComponent<Team>();
			team.team = gameMode.teams[teamId];
			team.enabled = true;
		}
		return newTower;
	}
}
