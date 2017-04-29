﻿using UnityEngine;

public class Teleport : MonoBehaviour {

	public AbstractPlayerSpawner target;

	private void OnTriggerEnter(Collider other) {
		Player player = other.transform.root.GetComponent<Player>();
		if (player != null && player.isLocalPlayer) {
			player.transform.position = target.nextSpawnPos();
		}
	}
}
