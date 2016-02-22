using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class RobotSpawner : NetworkBehaviour {

	private static int maxRobots = 1;
	private static float timeSinceLastSpawn = 0f;
	private static RobotSpawner activeSpawner;

	public bool active = false;
	public bool playerActivates = false;
	public float delay = 5f;
	public bool debug = false;

	public Transform bodyPrefab;
	public Transform armsPrefab;
	public Transform generatorPrefab;
	public Transform hoverPackPrefab;

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
		if((active || this == activeSpawner) && RobotController.controllerCount < maxRobots) {
			timeSinceLastSpawn += Time.deltaTime;
			if(timeSinceLastSpawn > delay) {
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

	[Server]
	private void spawnRobot() {


		if(bodyPrefab != null && armsPrefab != null && generatorPrefab != null && hoverPackPrefab != null) {
			Transform body = Instantiate(bodyPrefab, transform.position, bodyPrefab.transform.rotation) as Transform;
			Transform arms = Instantiate(armsPrefab, transform.position+armsPrefab.transform.position, armsPrefab.transform.rotation) as Transform;
			Transform generator = Instantiate(generatorPrefab, transform.position + generatorPrefab.transform.position, generatorPrefab.transform.rotation) as Transform;
			Transform hoverPack = Instantiate(hoverPackPrefab, transform.position + hoverPackPrefab.transform.position, hoverPackPrefab.transform.rotation) as Transform;

			generator.transform.parent = body;
			arms.transform.parent = body;
			hoverPack.transform.parent = body;

			WinZone winZone = FindObjectOfType<WinZone>();
			Player[] players = FindObjectsOfType<Player>();
            //body.GetComponent<RobotController>().locations = new Label[players.Length];
            Label[] labels = new Label[players.Length+1];
            for (int i = 0; i < players.Length; ++i) {
                Player player = players[i];
                if (player != null) {
                    labels[i] = player.GetComponent<Label>();
                } else if (player == null) {
                    Debug.LogWarning("Scene contains no player!!!");
                }

            }
            if (winZone != null) {
                labels[labels.Length - 1] = winZone.GetComponent<Label>();
                }
            else if (winZone == null) {
                Debug.LogWarning("Scene contains no win zone!!!");
            }
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
			hoverPack.GetComponent<AbstractRobotComponent>().setControllerId(robotId);
			//generator.GetComponent<AbstractRobotComponent>().setControllerId(robotId);
			arms.GetComponent<AbstractRobotComponent>().setControllerId(robotId);

			body.GetComponent<NavMeshAgent>().enabled = true;
		} else {
			print("Null");
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
