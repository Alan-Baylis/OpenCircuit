﻿using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

	private Player myPlayer;
	private Vector3 groundNormal;
	private Vector3 groundSpeed;
	private Vector3 desiredVel;
	private float forwardSpeed = 0;
	private float rightSpeed = 0;
	private bool grounded;
	private bool sprinting = false;
	//private bool crouching = false;
	private GameObject floor = null;
	private CapsuleCollider col;
	private AudioSource footstepEmitter;
	private float nextFootstep;

	// climbing stuff
	//private float minStepHeight = 0.1f;
	private float maxStepHeight = 1f;
	//private float minLedgeWidth = 0.25f;
	private float normHeight;
	private int freeFallDelay = 0;
	private bool recovering = false;
	private bool canMove = true;

	public float sprintMult = 2f;
	public float walkSpeedf = 6f;
	public float walkSpeedb = 4f;
	public float walkSpeedr = 4f;
	public float walkSpeedl = 4f;
	public float acceleration = 0.2f;
	public float climbRate = 0.1f;
	public float minCrouchHeight = 1;
	public float crouchHeight = 1.25f;
	public float jumpSpeed = 5;
	public float oxygenStopSprint = 10;
	public float oxygenBeginSprint = 15;
	public float oxygenSprintUsage = 30;
	public AudioClip[] footsteps;
	public float minimumFoostepOccurence;
	public float foostepSpeedScale;

	void Awake() {
		groundNormal = Vector3.zero;
		groundSpeed = Vector3.zero;
		myPlayer = GetComponent<Player>();
		col = GetComponent<CapsuleCollider>();
		normHeight = col.height;
		footstepEmitter = gameObject.AddComponent<AudioSource>();
		footstepEmitter.enabled = true;
		footstepEmitter.loop = false;
		nextFootstep = 0;
	}

	void FixedUpdate () {
		//float desiredHeight = crouching? crouchHeight: normHeight;
		//if (col.height < desiredHeight) {
		//	if (freeFallDelay == 0)
		//		setColliderHeight(col.height + climbRate /5);
		//	else
		//		setColliderHeight(col.height + climbRate);
		//	if (col.height > desiredHeight)
		//		setColliderHeight(desiredHeight);
		//} else if (col.height > desiredHeight) {
		//	setColliderHeight(col.height - climbRate);
		//	if (col.height < desiredHeight)
		//		setColliderHeight(desiredHeight);
		//}
		//else if (col.height != normHeight)
		//	setColliderHeight(normHeight);

		// move the player
		if (freeFallDelay < 0) {
			++freeFallDelay;
			return;
		}
		if (!isGrounded()) {
			if (freeFallDelay > 0)
				--freeFallDelay;
			else {
				nextFootstep = 1;
				return;
			}
		} else
			freeFallDelay = 2;

		// get the move direction and speed
		if (!canMove) {
			rightSpeed = 0;
			forwardSpeed = 0;
		}
		desiredVel = new Vector3(rightSpeed, 0, forwardSpeed);
		float speed = desiredVel.magnitude * (sprinting ? sprintMult : 1);
		float maxAccel = acceleration;
		desiredVel = myPlayer.cam.transform.TransformDirection(desiredVel);
		desiredVel.y = 0;

		// move parallel to the ground
		if (isGrounded()) {
			Vector3 sideways = Vector3.Cross(Vector3.up, desiredVel);
			desiredVel = Vector3.Cross(sideways, groundNormal).normalized;
		}
		desiredVel *= speed;

		// slow down when moving up a slope
		if (desiredVel.y > 0)
			desiredVel *= 1 - Mathf.Pow(desiredVel.y / speed, 2);
		if (!isGrounded())
			desiredVel.y = GetComponent<Rigidbody>().velocity.y;

		// handle the maximum acceleration
		Vector3 force = desiredVel -GetComponent<Rigidbody>().velocity +groundSpeed;
		if (force.magnitude > maxAccel) {
			force.Normalize();
			force *= maxAccel;
		}
		GetComponent<Rigidbody>().AddForce(force, ForceMode.VelocityChange);

		// play footstep sounds
		float currentSpeed = GetComponent<Rigidbody>().velocity.sqrMagnitude;
		if (nextFootstep <= Time.fixedTime && currentSpeed > 0.1f) {
			if (nextFootstep != 0) {
				float volume = 0.8f -(0.8f / (1 + currentSpeed /100));
				playFootstep(volume);
			}
			nextFootstep = Time.fixedTime + minimumFoostepOccurence / (1 + currentSpeed * foostepSpeedScale);
		}
		if (desiredVel.sqrMagnitude == 0 && currentSpeed < 1f && nextFootstep != 0) {
			float volume = 0.1f * currentSpeed;
			playFootstep(volume);
			nextFootstep = 0;
		}


		// update sprinting
		if (desiredVel.sqrMagnitude > 0.1f) {
			if (myPlayer.oxygen < oxygenStopSprint) {
				sprinting = false;
				recovering = true;
			} else if (sprinting) {
				myPlayer.oxygen -= oxygenSprintUsage * Time.deltaTime;
			}
		}



		// THE NEW STEP SYSTEM!!!
		//climb();


		// check for no more floor, in case it was deleted
		//if (floor == null)
			groundNormal = Vector3.zero;
	}

	private void playFootstep(float volume) {
		AudioClip clip = footsteps[Random.Range(0, footsteps.Length)];
		footstepEmitter.pitch = 0.9f +0.2f *Random.value;
		footstepEmitter.PlayOneShot(clip, volume);
	}

	private void doClimb(Collision collisionInfo) {
		//bool walkOver = false;
		//Vector3 vel = -collisionInfo.relativeVelocity;
		//if (collisionInfo.rigidbody != null)
		//	vel += collisionInfo.rigidbody.velocity;
		//foreach (ContactPoint cp in collisionInfo.contacts) {
		//	if (Vector3.Dot(cp.normal, vel) < -0.1f && Mathf.Abs(cp.normal.y) < 0.3f) {
		//		float height = ledgeCheck(desiredVel, maxStepHeight, minLedgeWidth);
		//		if (height > minStepHeight) {
		//			walkOver = true;
		//			setColliderHeight(Mathf.Min(normHeight - height, col.height));
		//		}
		//	}
		//}
		//if (walkOver) {
		//	rigidbody.velocity = vel;
		//}
	}

	private float ledgeCheck(Vector3 direction, float prefHeight, float ledgeWidth) {
		//RaycastHit hit;
		//bool isStep;
		//float simpleStepHeight = normHeight - minCrouchHeight;
		//float ledgeHeight = simpleStepHeight;
		//float maxHeight = prefHeight;

		//// position and generate the capsule collider
		//float capRad = col.radius -ledgeWidth;
		//Vector3 point1 = transform.position;
		//point1.y += normHeight / 2 - capRad;
		//Vector3 point2 = point1;
		//point2.y -= minCrouchHeight -capRad *2;

		//// check up
		////if (prefHeight > simpleStepHeight) {
		////	isStep = Physics.CapsuleCast(point1, point2, capRad, Vector3.up, out hit, prefHeight -simpleStepHeight);
		////	if (isStep) {
		////		maxHeight = hit.distance;
		////	}

		////	// the following two won't work once we implement the next step, the second up check
		////	point1.y += maxHeight - simpleStepHeight;
		////	point2.y += maxHeight - simpleStepHeight;
		////}

		////// check up on step
		////if (maxHeight > simpleStepHeight) {
		////	// yet to be implemented!!!  This will be needed for climbing and the like
		////}
		//direction.y = 0;

		//// check forward
		//isStep = Physics.CapsuleCast(point1, point2, capRad +0.1f, direction, out hit, ledgeWidth *3f);
		//if (isStep) {
		//	return 0;
		//}
		
		//// check down
		//point1 += direction.normalized * ledgeWidth *2;
		//point2 += direction.normalized * ledgeWidth *2;
		//isStep = Physics.CapsuleCast(point1, point2, capRad, -Vector3.up, out hit, maxHeight);
		//if (isStep) {
		//	return maxHeight -hit.distance;
		//}
		return 0;
	}

	void OnCollisionEnter(Collision collisionInfo) {
		doClimb(collisionInfo);
	}

	void OnCollisionStay(Collision collisionInfo) {
		foreach(ContactPoint cp in collisionInfo.contacts) {
			if ((floor == null && cp.normal.y > 0.6f) || cp.normal.y > groundNormal.y) {
				groundNormal = cp.normal;
				floor = collisionInfo.gameObject;
				if (collisionInfo.rigidbody != null)
					groundSpeed = collisionInfo.rigidbody.velocity;
				else
					groundSpeed = Vector3.zero;
			}
		}
		doClimb(collisionInfo);
	}

	void OnCollisionExit(Collision collisionInfo) {
		if (collisionInfo.gameObject == floor) {
			floor = null;
			groundNormal = Vector3.zero;
		}
	}

	public void setForward(float percent) {
		if (percent > 0)
			forwardSpeed = percent *walkSpeedf;
		else
			forwardSpeed = percent *walkSpeedb;
	}
	
	public void setRight(float percent) {
		if (percent > 0)
			rightSpeed = percent *walkSpeedr;
		else
			rightSpeed = percent *walkSpeedl;
	}
	
	public void setSprinting(bool sprint) {
		if (recovering && myPlayer.oxygen < oxygenBeginSprint)
			return;
		recovering = false;
		sprinting = sprint;
	}

	//public void setCrouching(bool crouch) {
	//	crouching = crouch;
	//}

	public void jump() {
		if (!canMove) return;
		if (!isGrounded ())
			return;
		Vector3 upSpeed = new Vector3(0, jumpSpeed, 0);
		if (floor.GetComponent<Rigidbody>() != null) upSpeed.y += floor.GetComponent<Rigidbody>().velocity.y;
		GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity + upSpeed;
		floor = null;
		setColliderHeight(normHeight - maxStepHeight);
	}

	private void setColliderHeight(float h) {
		//if (h < col.radius * 2) h = col.radius * 2;
		//col.center = col.center + new Vector3(0, (col.height - h) / 2, 0);
		//col.height = h;
	}
	
	public bool isGrounded() {
		return (floor != null && groundNormal.y > 0.6f) || (GetComponent<Rigidbody>().IsSleeping());
	}

	//void OnGUI() {
	//	GUI.Label(new Rect(100, 10, 100, 20), getGrounded().ToString());
	//	//if (desiredVel.sqrMagnitude > 0.01f)
	//	float thing = ledgeCheck(desiredVel, maxStepHeight, minLedgeWidth);
	//	if (thing > 0) GUI.Box(new Rect(100, 50, thing *100, 20), "");
	//	GUI.Box(new Rect(100, 30, 100, 20), thing.ToString());
	//}

	public void lockMovement() {
		canMove = false;
	}
}
