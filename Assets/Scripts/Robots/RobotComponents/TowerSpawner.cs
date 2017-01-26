using UnityEngine;
using UnityEngine.Networking;

public class TowerSpawner : AbstractRobotComponent {

	public GameObject towerPrefab;
	public Vector3 offset;


	[Server]
	public GameObject buildTower(Vector3 position) {
		GameObject newTower = GameObject.Instantiate(towerPrefab, position + offset, towerPrefab.transform.rotation) as GameObject;
		NetworkServer.Spawn(newTower);
		return newTower;
	}
}
