using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public abstract class AbstractRobotSpawner : NetworkBehaviour {

	public bool active = false;
	public bool debug = false;
	public bool spawnEyes = true;
	public bool playerOmniscient = false;

	public GameObject bodyPrefab;
	public GameObject armsPrefab;
	public GameObject generatorPrefab;
	public GameObject hoverPackPrefab;
	public GameObject eyesPrefab;

	private GlobalConfig config;



	[Server]
	protected void spawnRobot() {

		if (bodyPrefab != null && armsPrefab != null && generatorPrefab != null && hoverPackPrefab != null) {
			GameObject body = Instantiate(bodyPrefab, transform.position, bodyPrefab.transform.rotation) as GameObject;
			GameObject arms = Instantiate(armsPrefab, transform.position + armsPrefab.transform.position, armsPrefab.transform.rotation) as GameObject;
			GameObject generator = Instantiate(generatorPrefab, transform.position + generatorPrefab.transform.position, generatorPrefab.transform.rotation) as GameObject;
			GameObject hoverPack = Instantiate(hoverPackPrefab, transform.position + hoverPackPrefab.transform.position, hoverPackPrefab.transform.rotation) as GameObject;
			GameObject eyes = null;
			if(spawnEyes) {
				eyes = (Instantiate(eyesPrefab, transform.position + eyesPrefab.transform.position, eyesPrefab.transform.rotation)) as GameObject;
				eyes.transform.parent = body.transform;
			}

			generator.transform.parent = body.transform;
			arms.transform.parent = body.transform;
			hoverPack.transform.parent = body.transform;

			//WinZone winZone = FindObjectOfType<WinZone>();
			RobotController robotController = body.GetComponent<RobotController>();

			if(playerOmniscient) {
				applyPlayerKnowledge(robotController);
			}

			applySpawnerKnowledge(robotController);

#if UNITY_EDITOR
			robotController.debug = debug;
#endif

			body.gameObject.SetActive(true);
			hoverPack.gameObject.SetActive(true);
			arms.gameObject.SetActive(true);
			generator.gameObject.SetActive(true);

			if(spawnEyes) {
				eyes.gameObject.SetActive(true);
				NetworkServer.Spawn(eyes.gameObject);
			}

			NetworkServer.Spawn(body.gameObject);
			NetworkServer.Spawn(arms.gameObject);
			NetworkServer.Spawn(generator.gameObject);
			NetworkServer.Spawn(hoverPack.gameObject);


			NetworkInstanceId robotId = robotController.netId;
			hoverPack.GetComponent<NetworkParenter>().setParentId(robotId);
			generator.GetComponent<NetworkParenter>().setParentId(robotId);
			arms.GetComponent<NetworkParenter>().setParentId(robotId);

			body.GetComponent<NavMeshAgent>().enabled = true;
		} else {
			print("Null");
		}
	}



	protected GlobalConfig getConfig() {
		if (config == null) {
			config = FindObjectOfType<GlobalConfig>();
		}
		return config;
	}

	private void applyPlayerKnowledge(RobotController controller) {
		Player[] players = FindObjectsOfType<Player>();
		foreach(Player player in players) {
			controller.addKnownLocation(player.GetComponent<Label>());
		}
	}

	private void applySpawnerKnowledge(RobotController controller) {
		RobotSpawner[] spawners = FindObjectsOfType<RobotSpawner>();
		foreach(RobotSpawner spawner in spawners) {
			controller.addKnownLocation(spawner.GetComponent<Label>());
		}
	}
}
