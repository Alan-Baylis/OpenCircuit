using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

[AddComponentMenu("Scripts/Robot/Hover Jet")]
public class HoverJet : AbstractRobotComponent {

	public const string TARGET_REACHED = "target reached";

	public float distanceCost = 1;

	private LabelHandle target = null;
	private bool hasSentReachedMessage = false;

	private UnityEngine.AI.NavMeshAgent myNav;
	public UnityEngine.AI.NavMeshAgent nav { get {
			if (myNav == null)
				myNav = getController().GetComponent<UnityEngine.AI.NavMeshAgent>();
			return myNav;
		}
	}

	private bool matchTargetRotation = false;

	public float animSpeedAdjust = 1f;

	public float powerDrawRate = 5f;

	public float regularSpeed = 5f;
	public float pursueSpeed = 7f;

	public float speedRegenRate = 5f;
	public float heightRegenRate = 0.2f;

	private Vector3 ? targetLocation = null;
	private Vector3 ? targetDirection = null;

	[System.NonSerialized]
	public float speedMultipler = 1;

#if UNITY_EDITOR
	private GameObject dest;
#endif
	private float regularHeight;
	private float regularStrideLength;
	private LocomotionController chassis;

	public void goToPosition(Vector3? pos, bool autoBrake) {
		goToPosition(pos, null, autoBrake);
	}

	public void goToPosition(Vector3 ? pos, Vector3 ? dir, bool autoBrake) {
		stop();
		hasSentReachedMessage = false;
		matchTargetRotation = dir != null;
		targetDirection = dir;
		nav.autoBraking = autoBrake;
		if (pos != null) {
			targetLocation = pos;
			if (hasReachedTargetLocation(pos.Value) && hasMatchedTargetRotation(dir.Value)) {
				print("stuck?");
				target = null;
				targetDirection = null;
				return;
			}
			if (nav.enabled) {
				nav.Resume();
			}
		} else {
			Debug.Log(pos == null);
		}
	}

	public void setTarget(LabelHandle target, bool autoBrake, bool matchRotation = false) {
		stop();
		hasSentReachedMessage = false;
		matchTargetRotation = matchRotation;
		nav.autoBraking = autoBrake;
		if (target != null) {
			this.target = target;
			if (hasReachedTargetLocation(this.target) && hasMatchedTargetRotation()) {
				print("stuck?");
				this.target = null;
				return;
			}
			if (nav.enabled) {
				nav.Resume();
			}
		} else {
			Debug.Log("target? " + (target == null));
		}
	}

	public bool hasTarget() {
		return target != null;
	}

	[ServerCallback]
	public void Start() {
		chassis = GetComponentInChildren<LocomotionController>();
		regularSpeed += Random.Range(-0.5f, 0.5f);
		pursueSpeed += Random.Range(-0.5f, 0.5f);
		nav.speed = regularSpeed;
		regularHeight = nav.height;
	}

	[ServerCallback]
	void Update () {
//		double startTime = Time.realtimeSinceStartup;
		float actualSpeed = regularSpeed * speedMultipler;
		if(nav.speed < actualSpeed) {
			nav.speed += speedRegenRate * Time.deltaTime;
			if(nav.speed > actualSpeed) {
				nav.speed = actualSpeed;
			}
		} else if (nav.speed > actualSpeed) {
			nav.speed = actualSpeed;
		}

		if (nav.baseOffset < regularHeight) {
			nav.baseOffset = nav.baseOffset + heightRegenRate * Time.deltaTime;

			if (nav.baseOffset > regularHeight) {
				nav.baseOffset = regularHeight;
			}
		}
		if (powerSource == null) {
			Debug.LogWarning(getController().name + " is missing a power source.");
			return;
		}
		goToTarget();
		nav.enabled = powerSource.drawPower(powerDrawRate * Time.deltaTime);
//		double endTime = Time.realtimeSinceStartup;
//		getController().getExecutionTimer().addTime(endTime-startTime);
	}

	public float calculatePathCost(Label label) {
		return calculatePathCost(label.transform.position);
	}

	public float calculatePathCost(Vector3 targetPos) {

		UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath ();
		if (nav.enabled) {
			nav.CalculatePath (targetPos, path);
		}
		List<Vector3> corners = new List<Vector3>(path.corners);
		corners.Add(targetPos);
		float pathLength = 0;
		for (int i = 0; i < corners.Count; i++) {
			Vector3 vertex = corners[i];
			if (i > 0) {
				pathLength += Vector3.Distance(corners[i - 1], vertex);
			}
		}

		return (pathLength * distanceCost);	
	}

	public bool canReach(Label target) {
		return canReach(target.transform.position);
	}

	public bool canReach(Vector3 pos) {
		return true;
	}

	public bool hasReachedTarget(LabelHandle target) {
		return hasReachedTargetLocation(target.getPosition()) && hasMatchedTargetRotation(target.label.transform.forward);
	}

	public void stop() {
		target = null;
		targetLocation = null;
		targetDirection = null;
		matchTargetRotation = false;
		if (nav.enabled) {
			nav.Stop();
		}
	}

	public override void release() {
		stop();
	}

	private void goToTarget() {
		if(target != null || targetLocation != null) {
			Vector3 ? goal = target != null ? target.getPosition() : targetLocation.Value;
			goal = getNearestNavPos(goal.Value);
			if (goal != null) {
				if (hasReachedTargetLocation(goal.Value)) {
					if (target != null && !hasMatchedTargetRotation()) {
						getController().transform.rotation = Quaternion.RotateTowards(Quaternion.LookRotation(getController().transform.forward), Quaternion.LookRotation(target.label.transform.forward), nav.angularSpeed * Time.deltaTime);
					} else if (targetLocation != null && !hasMatchedTargetRotation(targetDirection.Value)) {
						getController().transform.rotation = Quaternion.RotateTowards(Quaternion.LookRotation(getController().transform.forward), Quaternion.LookRotation(targetDirection.Value), nav.angularSpeed * Time.deltaTime);
					} else if (!hasSentReachedMessage) {
						getController().enqueueMessage(new RobotMessage(TARGET_REACHED, target, goal.Value, null));
						hasSentReachedMessage = true;
					}
				} else {
					hasSentReachedMessage = false;
				}

				if (nav.enabled) {
					nav.SetDestination(goal.Value);
				}

#if UNITY_EDITOR
				if (getController().debug) {
					Destroy(dest);
					GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
					cube.transform.position = goal.Value;
					cube.GetComponent<MeshRenderer>().material.color = Color.green;
					Destroy(cube.GetComponent<BoxCollider>());
					cube.transform.localScale = new Vector3(.3f, .3f, .3f);
					dest = cube;
				}
#endif

			} else {
				//TODO: Send an action message if point is unreachable

			}
		}
	}

	private Vector3? getNearestNavPos(Vector3 pos) {
		UnityEngine.AI.NavMeshHit hit;
		UnityEngine.AI.NavMesh.SamplePosition(pos, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas);
		if (hit.hit) {
			return hit.position;
		} 
		return null;
	}

	private bool hasReachedTargetLocation(LabelHandle labelHandle) {
		return hasReachedTargetLocation(labelHandle.getPosition());
	}

	private bool hasReachedTargetLocation(Vector3 targetLocation) {
		float xzDist = Vector2.Distance(new Vector2(getController().transform.position.x, getController().transform.position.z),
			new Vector2(targetLocation.x, targetLocation.z));
		float yDist = Mathf.Abs((getController().transform.position.y - .4f) - targetLocation.y);
		if(xzDist < .5f && yDist < 2.5f) {
			return true;
		}
		return false;
	}

	private bool hasMatchedTargetRotation() {
		return hasMatchedTargetRotation(target.label.transform.forward);
	}

	private bool hasMatchedTargetRotation(Vector3 forward) {
		if(!matchTargetRotation) {
			return true;
		}
		return (1 - Vector3.Dot(getController().transform.forward, forward) < .0001f);
	}
}
