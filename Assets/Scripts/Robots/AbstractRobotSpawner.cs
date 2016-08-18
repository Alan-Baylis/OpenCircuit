﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public abstract class AbstractRobotSpawner : NetworkBehaviour {

	public bool active = false;
	public bool debug = false;
	public bool playerOmniscient = false;

	public GameObject bodyPrefab;
	public GameObject[] componentPrefabs;

	private GlobalConfig config;



	[Server]
	protected void spawnRobot() {

		if (bodyPrefab != null) {
			GameObject body = Instantiate(bodyPrefab, transform.position, bodyPrefab.transform.rotation) as GameObject;

			//WinZone winZone = FindObjectOfType<WinZone>();
			RobotController robotController = body.GetComponent<RobotController>();

			addKnowledge(robotController);
			applyAmmoKnowledge(robotController);
            applyAmmoSpawnerKnowledge(robotController);

#if UNITY_EDITOR
			robotController.debug = debug;
#endif

			List<GameObject> components = new List<GameObject>();
			foreach(GameObject prefab in componentPrefabs) {
				if (prefab == null) {
					Debug.LogWarning("Null robot component in spawner: " + name);
					continue;
				}
				GameObject component = Instantiate(prefab, transform.position + prefab.transform.position, prefab.transform.rotation) as GameObject;
				component.transform.parent = body.transform;
				components.Add(component);
			}

			body.gameObject.SetActive(true);
			foreach (GameObject component in components) {
				component.SetActive(true);
			}

			NetworkServer.Spawn(body.gameObject);
			NetworkInstanceId robotId = robotController.netId;
			foreach (GameObject component in components) {
				NetworkServer.Spawn(component);
				NetworkParenter parenter = component.GetComponent<NetworkParenter>();
				if (parenter == null) {
					Debug.LogWarning("Robot component does not have network parenter: " +component.name);
				} else {
					parenter.setParentId(robotId);
				}
			}

			NavMeshAgent navAgent = body.GetComponent<NavMeshAgent>();
			navAgent.avoidancePriority = Random.Range(0, 100);
			navAgent.enabled = true;

			if (config.getCRC() != null) {
				config.getCRC().forceAddListener(robotController);
			}
		} else {
			Debug.LogError("Null body prefab in spawner: " +name);
		}
	}

	protected virtual void addKnowledge(RobotController robotController) {
		if(playerOmniscient) {
			applyPlayerKnowledge(robotController);
		}

		applySpawnerKnowledge(robotController);
	}



	protected GlobalConfig getConfig() {
		if (config == null) {
			config = FindObjectOfType<GlobalConfig>();
		}
		return config;
	}

	protected void applyPlayerKnowledge(RobotController controller) {
		Player[] players = FindObjectsOfType<Player>();
		foreach(Player player in players) {
			controller.addKnownLocation(player.GetComponent<Label>());
		}
	}

	protected void applySpawnerKnowledge(RobotController controller) {
		RobotSpawner[] spawners = FindObjectsOfType<RobotSpawner>();
		foreach(RobotSpawner spawner in spawners) {
			controller.addKnownLocation(spawner.GetComponent<Label>());
		}
	}

	private void applyAmmoSpawnerKnowledge(RobotController controller) {
        PickupableSpawner[] spawners = FindObjectsOfType<PickupableSpawner>();
        foreach (PickupableSpawner spawner in spawners) {
			controller.addKnownLocation(spawner.GetComponent<Label>());
		}
	}

    private void applyAmmoKnowledge(RobotController controller) {
        AmmoPickup[] spawners = FindObjectsOfType<AmmoPickup>();
        foreach (AmmoPickup spawner in spawners) {
            controller.addKnownLocation(spawner.GetComponent<Label>());
        }
    }
}