﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ChainRobotSpawner : RobotSpawner {

	private static float timeSinceLastSpawn = 0f;
	private static ChainRobotSpawner activeSpawner;

	public bool playerActivates = false;

	private bool triggered = false;

	[ServerCallback]
	void Start() {
		if(active) {
			activeSpawner = this;
		}
	}

	[ServerCallback]
	void Update() {
		if(this != activeSpawner) {
			active = false;
		}
		if(active && RobotController.controllerCount < getConfig().getMaxRobots()) {
			timeSinceLastSpawn += Time.deltaTime;
			if(timeSinceLastSpawn > getConfig().getDelay()) {
				spawnRobot();
				timeSinceLastSpawn = 0f;
			}
		}
	}

	[ServerCallback]
	public void OnTriggerEnter(Collider other) {
		if(!triggered) {
			Object activator;
			if (playerActivates) {
				activator = other.gameObject.GetComponent<Player>();
			} else {
				activator = other.gameObject.GetComponent<RobotController>();
			}
			if (activator != null) {
				triggered = true;
				activeSpawner = this;
			}
		}
	}

	void OnDrawGizmos() {
		if(activeSpawner == this || active) {
			Gizmos.color = Color.green;
		} else {
			Gizmos.color = Color.red;
		}
		Gizmos.DrawWireSphere(transform.position, 1f);
		BoxCollider box = GetComponent<BoxCollider>();
		Gizmos.DrawWireCube(transform.TransformPoint( box.center), box.size);
	}
}
