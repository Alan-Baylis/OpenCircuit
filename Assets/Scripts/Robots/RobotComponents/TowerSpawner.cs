using UnityEngine;
using UnityEngine.Networking;

public class TowerSpawner : AbstractRobotComponent {

	public GameObject towerPrefab;
	public GameObject[] components;
	public Vector3 offset;


	[Server]
	public void buildTower(Vector3 position, ClientController owner) {
		GameObject newTower = Instantiate(towerPrefab, position + offset, towerPrefab.transform.rotation);

		RobotController controller = newTower.GetComponent<RobotController>();
		if (getController().GetComponent<TeamId>().enabled && GlobalConfig.globalConfig.gamemode is Bases) {
			Bases gameMode = (Bases)GlobalConfig.globalConfig.gamemode;
			int teamId = getController().GetComponent<TeamId>().id;
			gameMode.getCRC(teamId).forceAddListener(controller);
			TeamId team = newTower.GetComponent<TeamId>();
			team.id = teamId;
			team.enabled = true;
		}
		newTower.GetComponent<Score>().owner = owner;
		newTower.GetComponent<ScoreAgent>().owner = owner;
		Bases bases = GlobalConfig.globalConfig.gamemode as Bases;
		if (bases != null && owner != null) {
			bases.addTower(owner, newTower);
		}
		NetworkServer.Spawn(newTower);

		foreach (GameObject componentPrefab in components) {
			GameObject component = Instantiate(componentPrefab, position + offset + componentPrefab.transform.position, componentPrefab.transform.rotation);
			component.GetComponent<NetworkParenter>().setParentId(controller.netId);
			NetworkServer.Spawn(component);
		}

	}
}
