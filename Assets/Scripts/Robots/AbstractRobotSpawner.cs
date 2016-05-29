using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public abstract class AbstractRobotSpawner : NetworkBehaviour {

	public bool active = false;
	public bool debug = false;

	public Transform bodyPrefab;
	public Transform armsPrefab;
	public Transform generatorPrefab;
	public Transform hoverPackPrefab;

	private GlobalConfig config;



	[Server]
	protected void spawnRobot() {


		if (bodyPrefab != null && armsPrefab != null && generatorPrefab != null && hoverPackPrefab != null) {
			Transform body = Instantiate(bodyPrefab, transform.position, bodyPrefab.transform.rotation) as Transform;
			Transform arms = Instantiate(armsPrefab, transform.position + armsPrefab.transform.position, armsPrefab.transform.rotation) as Transform;
			Transform generator = Instantiate(generatorPrefab, transform.position + generatorPrefab.transform.position, generatorPrefab.transform.rotation) as Transform;
			Transform hoverPack = Instantiate(hoverPackPrefab, transform.position + hoverPackPrefab.transform.position, hoverPackPrefab.transform.rotation) as Transform;

			generator.transform.parent = body;
			arms.transform.parent = body;
			hoverPack.transform.parent = body;

			//WinZone winZone = FindObjectOfType<WinZone>();
			Player[] players = FindObjectsOfType<Player>();
			Label[] labels = new Label[players.Length];
			for (int i = 0; i < players.Length; ++i) {
				Player player = players[i];
				if (player != null) {
					labels[i] = player.GetComponent<Label>();
				} else if (player == null) {
					Debug.LogWarning("Scene contains no player!!!");
				}

			}
			//if (winZone != null) {
			//	labels[labels.Length - 1] = winZone.GetComponent<Label>();
			//}
			//else if (winZone == null) {
			//	Debug.LogWarning("Scene contains no win zone!!!");
			//}
			RobotController robotController = body.GetComponent<RobotController>();
			robotController.locations = labels;

#if UNITY_EDITOR
			robotController.debug = debug;
#endif

			body.gameObject.SetActive(true);
			hoverPack.gameObject.SetActive(true);
			arms.gameObject.SetActive(true);
			generator.gameObject.SetActive(true);

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
}
