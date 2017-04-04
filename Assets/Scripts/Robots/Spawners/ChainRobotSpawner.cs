using UnityEngine;
using UnityEngine.Networking;

public class ChainRobotSpawner : RobotSpawner {

	private static float timeSinceLastSpawn = 0f;
	private static ChainRobotSpawner activeSpawner;

	public bool playerActivates = false;

	private bool triggered;

	[ServerCallback]
	void Start() {
		if(active) {
			activeSpawner = this;
		}
	}

	[ServerCallback]
	new void Update() {
		if(this != activeSpawner) {
			active = false;
		}
		if(active && GlobalConfig.globalConfig.robotControllers < GlobalConfig.globalConfig.getMaxRobots()) {
			timeSinceLastSpawn += Time.deltaTime;
			if(timeSinceLastSpawn > GlobalConfig.globalConfig.getDelay()) {
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
				active = true;
			}
		}
	}

	protected override void addKnowledge(RobotController robotController) {
		if(playerOmniscient) {
			applyPlayerKnowledge(robotController);
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
