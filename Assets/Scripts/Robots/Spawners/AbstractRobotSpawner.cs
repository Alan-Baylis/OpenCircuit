using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public abstract class AbstractRobotSpawner : NetworkBehaviour {

	public bool active = false;
	public bool debug = false;
	public bool playerOmniscient = false;

	public GameObject bodyPrefab;
	public GameObject[] componentPrefabs;

    public int teamIndex;

    public TeamData team {
        get {
            if (isTeamMode()) {
                TeamGameMode gameMode = (TeamGameMode) GlobalConfig.globalConfig.gamemode;
                if (teamIndex >= 0 && teamIndex < gameMode.teams.Count)
                    myTeam = gameMode.teams[teamIndex];
            }
            return myTeam;
        }
    }

	private GlobalConfig config;
    private Label label;
    private TeamData myTeam;

    [ServerCallback]
    void Start() {

    }

    [ServerCallback]
    public virtual void Update() {
        if (active && !getLabel().hasTag(TagEnum.Active)) {
			Tag newTag = new Tag(TagEnum.Active, 0, getLabel().labelHandle);
            getLabel().setTag(newTag);

        } else if (!active && getLabel().hasTag(TagEnum.Active)) {
            getLabel().clearTag(TagEnum.Active);
        }
    }

    [Server]
	protected void spawnRobot() {
        ++RobotController.controllerCount;
		if (bodyPrefab != null) {
			GameObject body = Instantiate(bodyPrefab, transform.position, bodyPrefab.transform.rotation);

			//WinZone winZone = FindObjectOfType<WinZone>();
			RobotController robotController = body.GetComponent<RobotController>();
		    if (isTeamMode()) {
		        Team teamComponent = body.GetComponent<Team>();
		        teamComponent.team = team;
		        teamComponent.enabled = true;
		    }

			addKnowledge(robotController);

#if UNITY_EDITOR
			robotController.debug = debug;
#endif

			List<GameObject> components = new List<GameObject>();
			foreach(GameObject prefab in componentPrefabs) {
				if (prefab == null) {
					Debug.LogWarning("Null robot component in spawner: " + name);
					continue;
				}
				GameObject component = Instantiate(prefab, transform.position + prefab.transform.position, prefab.transform.rotation);
				component.transform.parent = body.transform;
				components.Add(component);
			}

			body.SetActive(true);
			foreach (GameObject component in components) {
				component.SetActive(true);
			}

			NetworkServer.Spawn(body);
			NetworkInstanceId robotId = robotController.netId;
			foreach (GameObject component in components) {
				NetworkServer.Spawn(component);
				NetworkParenter parenter = component.GetComponent<NetworkParenter>();
				if (parenter == null) {
					Debug.LogWarning("Robot component does not have network parenter: " +component.name);
				} else {
					parenter.setParentId(robotId);
				}
				foreach(NetworkIdentity id in component.GetComponentsInChildren<NetworkIdentity>()) {
					NetworkServer.Spawn(id.gameObject);
				}
			}

			UnityEngine.AI.NavMeshAgent navAgent = body.GetComponent<UnityEngine.AI.NavMeshAgent>();
			navAgent.avoidancePriority = Random.Range(0, 100);
			navAgent.enabled = true;

			if (GlobalConfig.globalConfig.getCRC() != null) {
			    GlobalConfig.globalConfig.getCRC().forceAddListener(robotController);
			}
		} else {
			Debug.LogError("Null body prefab in spawner: " +name);
		}
	}

	protected virtual void addKnowledge(RobotController robotController) {
		if(playerOmniscient) {
			applyPlayerKnowledge(robotController);
		}
    }

	protected void applyPlayerKnowledge(RobotController controller) {
		Player[] players = FindObjectsOfType<Player>();
		foreach(Player player in players) {
			Label playerLabel =player.GetComponent<Label>();
            controller.sightingFound(playerLabel.labelHandle, playerLabel.transform.position, playerLabel.GetComponent<Rigidbody>().velocity);
		}
	}

    private Label getLabel() {
        if (label == null) {
            label = GetComponent<Label>();
        }
        return label;
    }

    private bool isTeamMode() {
        return GlobalConfig.globalConfig.gamemode is TeamGameMode;
    }
}
