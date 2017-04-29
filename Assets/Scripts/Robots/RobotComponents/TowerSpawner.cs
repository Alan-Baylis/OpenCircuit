using UnityEngine;
using UnityEngine.Networking;

public class TowerSpawner : AbstractRobotComponent {

	public GameObject towerPrefab;
	public GameObject[] components;
	public Vector3 offset;


	[Server]
	public void buildTower(Vector3 position, BuildDirectiveTag towerBase) {
		RaycastHit hit;
		Vector3 buildPosition;
		if (Physics.Raycast(position, new Vector3(0, -1, 0), out hit, 3f)) {
			buildPosition = hit.point;
		} else {
			buildPosition = position;
			Debug.LogWarning("Tower spawned in air?");
		}

		GameObject newTower = Instantiate(towerPrefab, buildPosition + offset, towerPrefab.transform.rotation);

		RobotController controller = newTower.GetComponent<RobotController>();
		if (getController().GetComponent<TeamId>().enabled && GlobalConfig.globalConfig.gamemode is Bases) {
			Bases gameMode = (Bases) GlobalConfig.globalConfig.gamemode;
			int teamId = getController().GetComponent<TeamId>().id;
			gameMode.getCRC(teamId).forceAddListener(controller);
			TeamId team = newTower.GetComponent<TeamId>();
			team.id = teamId;
			team.enabled = true;
		}
		ClientController owner = towerBase.owner;
		newTower.GetComponent<Score>().owner = owner;
		newTower.GetComponent<ScoreAgent>().owner = owner;
		Bases bases = GlobalConfig.globalConfig.gamemode as Bases;
		if (bases != null && owner != null) {
			bases.addTower(owner, newTower, towerBase.getLabelHandle().label.gameObject);
		}
		NetworkServer.Spawn(newTower);

		foreach (GameObject componentPrefab in components) {
			GameObject component = Instantiate(componentPrefab, buildPosition + offset + componentPrefab.transform.position,
				componentPrefab.transform.rotation);
			component.GetComponent<NetworkParenter>().setParentId(controller.netId);
			NetworkServer.Spawn(component);
		}
	}
}
