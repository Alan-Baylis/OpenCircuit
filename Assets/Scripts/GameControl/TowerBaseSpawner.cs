using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TowerBaseSpawner : MonoBehaviour {
	
	public Label towerBasePrefab;

	public float spawnDelay = 30f;

	private Label towerBase;
	private float timeRemaining;
	private bool inactive;

	private void Start() {
		timeRemaining = spawnDelay;
	}

	private void Update() {
		if (timeRemaining > 0 && !inactive) {
			timeRemaining -= Time.deltaTime;
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (towerBase != null)
			return;
		RobotController robotController = other.transform.root.GetComponent<RobotController>();
		if (robotController != null) {
			int team = robotController.GetComponent<TeamId>().id;
			if (GlobalConfig.globalConfig.teamGameMode.getJoinedPlayerCount(team) == 0) {
				Collider[] objects = Physics.OverlapSphere(transform.position, GetComponent<SphereCollider>().radius);
				foreach (Collider item in objects) {
					//This is cheating...we have no clean way to identify a tower
					if (item.transform.root.GetComponent<ScoreAgent>() != null) {
						timeRemaining = 30f;
						inactive = true;
						return;
					}
				}
				inactive = false;
				if (timeRemaining > 0)
					return;
				towerBase = Instantiate(towerBasePrefab, transform.position, towerBasePrefab.transform.rotation);
				NetworkServer.Spawn(towerBase.gameObject);
				((Bases)GlobalConfig.globalConfig.gamemode).getCRC(team).sightingFound(towerBase.labelHandle, towerBase.transform.position, null);
				EventManager.getInGameChannel().broadcastEvent(new TowerBaseSpawnEvent(team));
			}
		}
	}

	public class TowerBaseSpawnEvent : AbstractEvent {
		private int team;

		public TowerBaseSpawnEvent(int team) {
			this.team = team;
		}

		public int getTeam() {
			return team;
		}
	}
}
