using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public abstract class AbstractRobotSpawner : NetworkBehaviour {

	public bool active = false;
	public bool debug = false;
	public bool playerOmniscient = false;

	public GameObject bodyPrefab;
	public GameObject[] componentPrefabs;

    public Vector3 spawnPos;

	[SyncVar(hook = "changeDisplayColor")]
	private Color displayColor;

    private Vector3 worldSpawnPos {
        get { return transform.position + transform.TransformDirection(spawnPos); }
    }

	private GlobalConfig config;
    private Label label;

	private TeamId myTeamId;

	public TeamId teamId {
		get { return myTeamId ?? (myTeamId = GetComponent<TeamId>()); }
	}

    [ServerCallback]
    void Start() {
        if (isTeamMode()) {
            teamId.enabled = true;
			displayColor = teamId.team.config.color;
        }
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
		if (bodyPrefab != null) {
			GameObject body = Instantiate(bodyPrefab, worldSpawnPos, bodyPrefab.transform.rotation);

			//WinZone winZone = FindObjectOfType<WinZone>();
			RobotController robotController = body.GetComponent<RobotController>();
		    if (isTeamMode()) {
		        TeamId teamIdComponent = body.GetComponent<TeamId>();
		        teamIdComponent.id = teamId.id;
		        teamIdComponent.enabled = true;
		    }
			GlobalConfig.globalConfig.addRobotCount(robotController);

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
				GameObject component = Instantiate(prefab, worldSpawnPos + prefab.transform.position, prefab.transform.rotation);
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

			if (GetComponent<TeamId>().enabled && GlobalConfig.globalConfig.gamemode is Bases) {
			    ((Bases)GlobalConfig.globalConfig.gamemode).getCRC(teamId.id).forceAddListener(robotController);
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

	private void changeDisplayColor(Color color) {
		displayColor = color;
		Renderer renderer = GetComponent<Renderer>();
		if (renderer != null) {
			Material mat = renderer.material;

			mat.SetColor("_EmissionColor", displayColor);
			mat.SetColor("_Albedo", displayColor);
		}
	}

    void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(worldSpawnPos, .3f);
    }
}
