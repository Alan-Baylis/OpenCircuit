using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Scripts/Robot/Hover Jet")]
public class HoverJet : AbstractRobotComponent {

	public float distanceCost = 1;

	private LabelHandle target = null;

	private NavMeshAgent nav;
	
	private Animation myAnimator;

	private bool matchTargetRotation = false;
	private bool isPursuit = false;

	public float animSpeedAdjust = 1f;

    public float powerDrawRate = 5f;

	public float regularSpeed = 5f;
	public float pursueSpeed = 7f;

	public float speedRegenRate = 5f;
	public float heightRegenRate = 0.2f;

#if UNITY_EDITOR
	private GameObject dest;
#endif
	private float regularHeight;
	private float regularStrideLength;
	private ChassisController chassis;

	public void setTarget(LabelHandle target, bool autoBrake, bool matchRotation = false) {
		this.target = target;
		matchTargetRotation = matchRotation;
		if(target == null) {
			if(nav.enabled) {
				nav.Stop();
			}
			isPursuit = false;
		} else {
			if(hasReachedTargetLocation() && hasMatchedTargetRotation()) {
				this.target = null;
				//print("bailing...");
				return;
			}
			if(nav.enabled) {
				nav.Resume();
			}
		}
		nav.autoBraking = autoBrake;
	}

	public void pursueTarget(LabelHandle target, bool autoBrake) {
		setTarget(target, autoBrake);
		isPursuit = true;
	}

	public bool hasTarget() {
		return target != null;
	}

	[ServerCallback]
	void Start() {
		myAnimator = GetComponent<Animation> ();
		nav = roboController.GetComponent<NavMeshAgent> ();
		chassis = GetComponentInChildren<ChassisController>();
		regularSpeed += Random.Range(-0.5f, 0.5f);
		pursueSpeed += Random.Range(-0.5f, 0.5f);
		nav.speed = regularSpeed;
		regularHeight = nav.height;
		regularStrideLength = chassis.strideLength;
	}

	[ServerCallback]
	void Update () {
		if(nav.speed < regularSpeed) {
			nav.speed = nav.speed + speedRegenRate * Time.deltaTime;

			if(nav.speed > regularSpeed) {
				nav.speed = regularSpeed;
			}
		}
		chassis.strideLength = regularStrideLength *nav.speed /regularSpeed;
		if (nav.baseOffset < regularHeight) {
			nav.baseOffset = nav.baseOffset + heightRegenRate * Time.deltaTime;

			if (nav.baseOffset > regularHeight) {
				nav.baseOffset = regularHeight;
			}
		}
		if (powerSource == null) {
			Debug.LogWarning(roboController.name + " is missing a power source.");
			return;
		}
		if(isPursuit) {
			pursueTarget();
		} else {
			goToTarget();
		}
		nav.enabled = powerSource.drawPower(powerDrawRate * Time.deltaTime);
	}

	public float calculatePathCost(Label label) {
		return calculatePathCost(label.transform.position);
	}

	public float calculatePathCost(Vector3 targetPos) {
		//Debug.Log ("evaluating path cost");
		//Debug.Log(targetPos);
		float cost = 0;
		NavMeshPath path = new NavMeshPath ();
		if (nav.enabled) {
			nav.CalculatePath (targetPos, path);
		}
		List<Vector3> corners = new List<Vector3>(path.corners);
		corners.Add(targetPos);
		//corners
		float pathLength = 0;
		foreach (LabelHandle item in roboController.getTrackedTargets()) {
			//print ("checking path cost against item: " + item.name);
			//print ("target threatLevel " + item.threatLevel);
			float minDist = -1;
			//Vector3 prevVertex;
			//Debug.Log("numCorners: " + corners.Count);
			for(int i = 0; i < corners.Count; i++) {
				Vector3 vertex = corners[i];
				if(i > 0) {
					//Debug.Log("adding path length");
					pathLength += Vector3.Distance(corners[i - 1], vertex);
				}
				float curDist = Vector3.Distance(vertex, item.getPosition());
				if(minDist == -1) {
					minDist = curDist;
				} else if(curDist < minDist) {
					minDist = curDist;
				}
			}
			if(item.hasTag(TagEnum.Threat)) {
				float threatLevel = item.getTag(TagEnum.Threat).severity;

				RoboEyes eyes = roboController.GetComponentInChildren<RoboEyes>();
				if(eyes != null) {
					cost += threatLevel * (minDist/eyes.sightDistance);	
				}
			}
		}
		//if (cost > 0) {
		//	print ("path cost: " + cost);
		//}
		//Debug.Log("cost for target " + cost +" "+ "("+pathLength +"*" + distanceCost+")");
		return cost + (pathLength * distanceCost);	
	}

	public bool canReach(Label target) {
		return canReach(target.transform.position);
	}

	public bool canReach(Vector3 pos) {
		//if(nav.enabled) {
		//	//Debug.Log("got here");
		//	NavMeshPath path = new NavMeshPath();

		//	nav.CalculatePath(pos, path);
		//	List<Vector3> corners = new List<Vector3>(path.corners);
		//	corners.Add(pos);
		//	for(int i = 0; i < corners.Count - 1; i++) {
		//		NavMeshHit hit = new NavMeshHit();

		//		if(NavMesh.Raycast(corners[i], corners[i + 1], out hit, NavMesh.AllAreas)) {
		//			return false;
		//		}
		//	}
		//	return true;
		//} else {
		//	return false;
		//}
		return true;
	}

	public bool hasReachedTarget(LabelHandle target) {
		return hasReachedTargetLocation(target) && hasMatchedTargetRotation(target);
	}

	private void goToTarget() {
		if(target != null) {

			if(hasReachedTargetLocation()) {
				//print("Target reached...matching rotation");
				if(!hasMatchedTargetRotation()) {
					//print("attempting to match target rotation");
					roboController.transform.rotation = Quaternion.RotateTowards(Quaternion.LookRotation(roboController.transform.forward), Quaternion.LookRotation(target.label.transform.forward), nav.angularSpeed * Time.deltaTime);
				} else {
					//print("rotation matched");
					roboController.enqueueMessage(new RobotMessage(RobotMessage.MessageType.ACTION, "target reached", target, target.getPosition(), null));
					target = null;
					nav.Stop();
					return;
				}
			}

			if(nav.enabled) {
				//nav.speed = regularSpeed;
				nav.SetDestination(target.getPosition());

#if UNITY_EDITOR
				if(roboController.debug) {
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

	private void pursueTarget() {
		if(target != null) {
			if(hasReachedTargetLocation()) {
				if(!hasMatchedTargetRotation()) {
					roboController.transform.rotation = Quaternion.RotateTowards(Quaternion.LookRotation(roboController.transform.forward), Quaternion.LookRotation(target.label.transform.forward), nav.angularSpeed * Time.deltaTime);
				} else {
					roboController.enqueueMessage(new RobotMessage(RobotMessage.MessageType.ACTION, "target reached", target, target.getPosition(), null));
					target = null;
					return;
				}
			}

			if(nav.enabled) {
				//nav.speed = pursueSpeed;
				if(target.getDirection().HasValue && Vector3.Distance(roboController.transform.position, target.getPosition()) > target.getDirection().Value.magnitude) {
					nav.SetDestination((target.getPosition()));// + 
						//target.getDirection().Value
						//* .08f
						//* (1 + Vector3.Dot(target.getDirection().Value.normalized, (target.getPosition() - roboController.transform.position).normalized))
						//*(target.getDirection().Value.magnitude/nav.speed) 
						//* Vector3.Distance(roboController.transform.position, target.getPosition())));
				} else {
					nav.SetDestination(target.getPosition());
				}

#if UNITY_EDITOR
				if(roboController.debug) {
					Destroy(dest);
					GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
					cube.transform.position = nav.destination;
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

			myAnimator["Armature.003|walk"].speed = nav.velocity.magnitude * animSpeedAdjust;//, nav.velocity * animSpeedAdjust, nav.velocity * animSpeedAdjust);
		}
	}

	private bool hasReachedTargetLocation() {
		return hasReachedTargetLocation(target);
	}

	private bool hasReachedTargetLocation(LabelHandle targetLocation) {
		float xzDist = Vector2.Distance(new Vector2(roboController.transform.position.x, roboController.transform.position.z),
								new Vector2(targetLocation.getPosition().x, targetLocation.getPosition().z));
		float yDist = Mathf.Abs((roboController.transform.position.y - .4f) - targetLocation.getPosition().y);
		if(xzDist < .5f && yDist < .8f) {
			return true;
		}
		return false;
	}

	private bool hasMatchedTargetRotation() {
		return hasMatchedTargetRotation(target);
	}

	private bool hasMatchedTargetRotation(LabelHandle targetRotation) {
		if(!matchTargetRotation) {
			//print("not attempting to match target rotation");
			return true;
		}
		return (1 - Vector3.Dot(roboController.transform.forward, targetRotation.label.transform.forward) < .0001f);
	}
}
