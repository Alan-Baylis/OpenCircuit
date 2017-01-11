using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

[AddComponentMenu("Scripts/Robot/Hover Jet")]
public class HoverJet : AbstractRobotComponent {

    public const string TARGET_REACHED = "target reached";

	public float distanceCost = 1;

	private LabelHandle target = null;

	private NavMeshAgent myNav;
	public NavMeshAgent nav { get {
			if (myNav == null)
				myNav = getController().GetComponent<NavMeshAgent>();
			return myNav;
		}
	}

	private Animation myAnimator;

	private bool matchTargetRotation = false;

	public float animSpeedAdjust = 1f;

    public float powerDrawRate = 5f;

	public float regularSpeed = 5f;
	public float pursueSpeed = 7f;

	public float speedRegenRate = 5f;
	public float heightRegenRate = 0.2f;

	private Vector3 ? targetLocation = null;

	[System.NonSerialized]
	public float speedMultipler = 1;

#if UNITY_EDITOR
	private GameObject dest;
#endif
	private float regularHeight;
	private float regularStrideLength;
	private ChassisController chassis;

	public void goToPosition(Vector3 ? pos, bool autoBrake) {
		stop();
		nav.autoBraking = autoBrake;
		if (pos != null) {
			targetLocation = pos;
			if (hasReachedTargetLocation(pos.Value)) {
				this.target = null;
				return;
			}
			if (nav.enabled) {
				nav.Resume();
			}
		}
	}

	public void setTarget(LabelHandle target, bool autoBrake, bool matchRotation = false) {
		stop();
		matchTargetRotation = matchRotation;
		nav.autoBraking = autoBrake;
		if (target != null) {
            this.target = target;
			if(hasReachedTargetLocation(this.target) && hasMatchedTargetRotation()) {
				this.target = null;
				return;
			}
			if(nav.enabled) {
				nav.Resume();
			}
		}
	}

	public bool hasTarget() {
		return target != null;
	}

	[ServerCallback]
	public void Start() {
		myAnimator = GetComponent<Animation>();
		chassis = GetComponentInChildren<ChassisController>();
		regularSpeed += Random.Range(-0.5f, 0.5f);
		pursueSpeed += Random.Range(-0.5f, 0.5f);
		nav.speed = regularSpeed;
		regularHeight = nav.height;
		regularStrideLength = chassis.strideLength;
	}

	[ServerCallback]
	void Update () {
		float actualSpeed = regularSpeed * speedMultipler;
		if(nav.speed < actualSpeed) {
			nav.speed += speedRegenRate * Time.deltaTime;
			if(nav.speed > actualSpeed) {
				nav.speed = actualSpeed;
			}
		} else if (nav.speed > actualSpeed) {
			nav.speed = actualSpeed;
        }

		chassis.strideLength = regularStrideLength *nav.speed /actualSpeed;
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
	}

	public float calculatePathCost(Label label) {
		return calculatePathCost(label.transform.position);
	}

	public float calculatePathCost(Vector3 targetPos) {

		NavMeshPath path = new NavMeshPath ();
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
        this.target = null;
		this.targetLocation = null;
        if (nav.enabled) {
            nav.Stop();
        }
    }

	public override void release() {
		stop();
	}

	private void goToTarget() {
		if(target != null) {
			if(hasReachedTargetLocation(target)) {
				if(!hasMatchedTargetRotation()) {
					getController().transform.rotation = Quaternion.RotateTowards(Quaternion.LookRotation(getController().transform.forward), Quaternion.LookRotation(target.label.transform.forward), nav.angularSpeed * Time.deltaTime);
				} else {
					getController().enqueueMessage(new RobotMessage(RobotMessage.MessageType.ACTION, TARGET_REACHED, target, target.getPosition(), null));
					target = null;
					nav.Stop();
					return;
				}
			}

			if(nav.enabled) {
				if (targetLocation != null) {
					print("Setting nav destination");
					nav.SetDestination(targetLocation.Value);
				} else {
					nav.SetDestination(target.getPosition());
				}

#if UNITY_EDITOR
				if (getController().debug) {
					Destroy(dest);
					GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
					cube.transform.position = target.getPosition();
					cube.GetComponent<MeshRenderer>().material.color = Color.green;
					Destroy(cube.GetComponent<BoxCollider>());
					cube.transform.localScale = new Vector3(.3f, .3f, .3f);
					dest = cube;
				}
#endif
			}
		}
	}

	private void animate() {
		if(myAnimator != null) {
			if(!myAnimator.isPlaying) {
				myAnimator.Play();
			}

			myAnimator["Armature.003|walk"].speed = nav.velocity.magnitude * animSpeedAdjust;
		}
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
