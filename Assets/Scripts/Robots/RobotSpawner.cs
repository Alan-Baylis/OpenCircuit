using UnityEngine;
using UnityEditor;
using System.Collections;

public class RobotSpawner : MonoBehaviour {

	private static int maxRobots = 10;
	private static float timeSinceLastSpawn = 0f;
	private static RobotSpawner activeSpawner;

	public bool active = false;
	public float delay = 5f;
	public bool debug = false;

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
			RobotController robot = other.gameObject.GetComponent<RobotController>();
			if(robot != null) {
				triggered = true;
				activeSpawner = this;
			}
		}
	}

	private void spawnRobot() {
		GameObject bodyPrefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Robots/Robot.prefab", typeof(GameObject)) as GameObject;
		GameObject armsPrefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Robots/ZappyArms.prefab", typeof(GameObject)) as GameObject;
		GameObject generatorPrefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Robots/Generator.prefab", typeof(GameObject)) as GameObject;
		GameObject hoverPackPrefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Robots/HoverPack.prefab", typeof(GameObject)) as GameObject;


		if(bodyPrefab != null && armsPrefab != null && generatorPrefab != null && hoverPackPrefab != null) {
			GameObject body = Instantiate(bodyPrefab, transform.position, bodyPrefab.transform.rotation) as GameObject;
			GameObject arms = Instantiate(armsPrefab, transform.position+armsPrefab.transform.position, armsPrefab.transform.rotation) as GameObject;
			GameObject generator = Instantiate(generatorPrefab, transform.position + generatorPrefab.transform.position, generatorPrefab.transform.rotation) as GameObject;
			GameObject hoverPack = Instantiate(hoverPackPrefab, transform.position + hoverPackPrefab.transform.position, hoverPackPrefab.transform.rotation) as GameObject;

			generator.transform.parent = body.transform;
			arms.transform.parent = body.transform;
			hoverPack.transform.parent = body.transform;

			body.GetComponent<RobotController>().locations = new Label[2] { FindObjectOfType<Player>().GetComponent<Label>(), FindObjectOfType<WinZone>().GetComponent<Label>() };
			body.GetComponent<RobotController>().debug = debug;
			body.SetActive(true);
			hoverPack.SetActive(true);
			arms.SetActive(true);
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
