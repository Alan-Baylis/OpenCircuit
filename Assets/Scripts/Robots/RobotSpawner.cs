using UnityEngine;
using UnityEditor;
using System.Collections;

public class RobotSpawner : MonoBehaviour {

	public int maxRobots = 0;
	public bool active = false;
	public float delay = 5f;
	public bool debug = false;
	private float timeSinceLastSpawn = 0f;
	private int robotCount;

	void Update() {
		if(active) {
			timeSinceLastSpawn += Time.deltaTime;
			if(robotCount < maxRobots && timeSinceLastSpawn > delay) {
				spawnRobot();
				timeSinceLastSpawn = 0f;
			}
		}
	}

	public void OnTriggerEnter(Collider other) {
		Player player = other.gameObject.GetComponent<Player>();
		if(player != null) {
			active = true;
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

			body.GetComponent<RobotController>().locations = new Label[1] { FindObjectOfType<Player>().GetComponent<Label>() };
			body.GetComponent<RobotController>().debug = debug;
			body.SetActive(true);
			hoverPack.SetActive(true);
			arms.SetActive(true);
			robotCount++;
		} else {
			print("Null");
		}
	}
}
