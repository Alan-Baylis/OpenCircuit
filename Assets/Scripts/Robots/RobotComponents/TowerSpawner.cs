using UnityEngine;
using UnityEngine.Networking;

public class TowerSpawner : AbstractRobotComponent {

	public GameObject towerPrefab;

	[Server]
	public GameObject buildTower(Vector3 position) {
		GameObject newTower = GameObject.Instantiate(towerPrefab, position, towerPrefab.transform.rotation) as GameObject;
		NetworkServer.Spawn(newTower);
		return newTower;
	}
}
