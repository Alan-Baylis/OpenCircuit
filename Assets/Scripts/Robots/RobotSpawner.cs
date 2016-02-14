using UnityEngine;
using System.Collections;

public class RobotSpawner : MonoBehaviour {

	private static int maxRobots = 10;
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

	void Start() {
		if(active) {
			activeSpawner = this;
		}
	}

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

	private void spawnRobot() {


		if(bodyPrefab != null && armsPrefab != null && generatorPrefab != null && hoverPackPrefab != null) {
			Transform body = Instantiate(bodyPrefab, transform.position, bodyPrefab.transform.rotation) as Transform;
			Transform arms = Instantiate(armsPrefab, transform.position+armsPrefab.transform.position, armsPrefab.transform.rotation) as Transform;
			Transform generator = Instantiate(generatorPrefab, transform.position + generatorPrefab.transform.position, generatorPrefab.transform.rotation) as Transform;
			Transform hoverPack = Instantiate(hoverPackPrefab, transform.position + hoverPackPrefab.transform.position, hoverPackPrefab.transform.rotation) as Transform;

			generator.transform.parent = body;
			arms.transform.parent = body;
			hoverPack.transform.parent = body;

			body.GetComponent<RobotController>().locations = new Label[2] { FindObjectOfType<Player>().GetComponent<Label>(), FindObjectOfType<WinZone>().GetComponent<Label>() };
#if UNITY_EDITOR
			body.GetComponent<RobotController>().debug = debug;
#endif
			body.gameObject.SetActive(true);
			hoverPack.gameObject.SetActive(true);
			arms.gameObject.SetActive(true);
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
