using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PickupableSpawner : NetworkBehaviour {

	public GameObject spawnedPrefab;
	public float spawnDelay = 10f;

	private GameObject spawnedObject;
	private float spawnTimeRemaining = 0f;
	private bool isSpawned = false;

	[ServerCallback]
	void Start() {
		spawnObject();
	}

	[ServerCallback]
	void Update() {
		if(isSpawned && spawnedObject == null) {
			isSpawned = false;
			spawnTimeRemaining = spawnDelay;
		} else if(spawnedObject == null && spawnTimeRemaining > 0) {
			spawnTimeRemaining -= Time.deltaTime;
			if(spawnTimeRemaining <= 0) {
				spawnObject();
			}
		}
	}

	[Server]
	private void spawnObject() {
		isSpawned = true;
		spawnedObject = Instantiate(spawnedPrefab, transform.position, transform.rotation) as GameObject;
		NetworkServer.Spawn(spawnedObject);
	}

}
