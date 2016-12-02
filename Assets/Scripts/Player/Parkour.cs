using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Scripts/Player/Parkour")]
public class Parkour : MovementController {

	private Player myPlayer;

	public Player player {
		get {
			if(myPlayer == null) {
				myPlayer = GetComponent<Player>();
			}
			return myPlayer; 
		}
	}
	private List<FloorData> floors;
	private int highestFlatFloor;
	private int flatestFloor;
	private Vector3 navPoint;
	private float forwardSpeed = 0;
	private float rightSpeed = 0;
	private bool sprinting = false;
	private bool crouching = false;
	private bool autoMove = false;
	private CapsuleCollider col;
	private AudioSource footstepEmitter;
	private float nextFootstep;

	// climbing stuff
	private int freeFallDelay = 0;
	private bool canMove = true;
	private Vector3 lastRelativeVelocity = Vector3.zero;
	private int collisionCount = 0;
	private List<float> pastForces = new List<float>();
	private List<int> pastForcesAddedCount = new List<int>();
	private int pastForceCount = 5;
	private int lastForceCount = 0;
	private Rigidbody rb;

	public float sprintMult = 2f;
	public float walkSpeedf = 6f;
	public float walkSpeedb = 4f;
	public float walkSpeedr = 4f;
	public float walkSpeedl = 4f;
	public float acceleration = 0.2f;
	public float climbRate = 0.1f;
	public float crouchSpeedMultiplier = 0.5f;
	public float crouchHeight = 1.25f;
	public float standingElevation = 0.65f;
	public float crouchingElevation = 0.25f;
	public float jumpSpeed = 5;
	public float airControl = 0.1f;
	public float oxygenStopSprint = 10;
	public float oxygenBeginSprint = 15;
	public float oxygenSprintUsage = 30;
	public AudioClip[] footsteps;
	public float minimumFoostepOccurence;
	public float foostepSpeedScale;
	public float fallHurtSpeed = 10;
	public float fallDeathSpeed = 15;

	void Awake() {
		floors = new List<FloorData>();
		col = GetComponent<CapsuleCollider>();
		rb = GetComponent<Rigidbody>();
		footstepEmitter = gameObject.AddComponent<AudioSource>();
		footstepEmitter.enabled = true;
		footstepEmitter.loop = false;
		nextFootstep = 0;
	}

	void FixedUpdate() {
		// change collider height
		float desiredHeight = crouchHeight;
		if (col.height < desiredHeight) {
			if (freeFallDelay == 0)
				setColliderHeight(col.height + climbRate /5);
			else
				setColliderHeight(col.height + climbRate);
			if (col.height > desiredHeight)
				setColliderHeight(desiredHeight);
		} else if (col.height > desiredHeight) {
			setColliderHeight(col.height - climbRate);
			if (col.height < desiredHeight)
				setColliderHeight(desiredHeight);
		}

		//check if grounded
		float dHeight = crouching ? crouchingElevation : standingElevation;
		detectFloor();

		// update past forces
		int newForceCount = pastForces.Count -lastForceCount;
		pastForcesAddedCount.Add(newForceCount);
		if (pastForcesAddedCount.Count > pastForceCount) {
			int forcesToRemove = pastForcesAddedCount[0];
			pastForcesAddedCount.RemoveAt(0);
			pastForces.RemoveRange(0, forcesToRemove);
		}
		for (int i = 0; i < pastForces.Count; ++i)
			pastForces[i] *= 0.8f;
		lastForceCount = pastForces.Count;

		// move the player
		if (freeFallDelay < 0) {
			++freeFallDelay;
			return;
		}
		float actualAcceleration = acceleration;
		if (!isGrounded()) {
			if (freeFallDelay > 0)
				--freeFallDelay;
			else {
				nextFootstep = 1;
				actualAcceleration *= airControl;
				//return;
			}
		} else
			freeFallDelay = 2;

		// get the move direction and speed
		if (!canMove) {
			rightSpeed = 0;
			forwardSpeed = 0;
		}
		Vector3 desiredVel = getDesiredVel();

		float crouchingSpeedMult = isGrounded() ?
			1 -(1 -crouchSpeedMultiplier) *Mathf.Max((standingElevation -floors[highestFlatFloor].elevation) /(standingElevation -crouchingElevation), 0):
			1;
		float speed = desiredVel.magnitude * (sprinting ? calculateSprintMultiplier() : 1) *crouchingSpeedMult;
		desiredVel.y = 0;
		desiredVel.Normalize();

		// move parallel to the ground
		if (isGrounded()) {
			Vector3 sideways = Vector3.Cross(Vector3.up, desiredVel);
			desiredVel = Vector3.Cross(sideways, floors[highestFlatFloor].normal).normalized;
		}
		desiredVel *= speed;

		// slow down when moving up a slope
		if (desiredVel.y > 0)
			desiredVel *= 1 - Mathf.Pow(desiredVel.y / speed, 4);
		if (!isGrounded())
			desiredVel.y = rb.velocity.y;

		if (isGrounded()) {
			// levitate
			desiredVel.y += Mathf.Clamp((dHeight - floors[highestFlatFloor].elevation) * 30 *actualAcceleration, -4 * actualAcceleration, 4 * actualAcceleration);

			// keep player from flying
			if (floors[highestFlatFloor].elevation > dHeight)
				desiredVel.y = rb.velocity.y;

			// slide down slopes
			Vector3 sideways = Vector3.Cross(Vector3.up, floors[flatestFloor].normal);
			Vector3 downSlope = Vector3.Cross(sideways, floors[flatestFloor].normal);
			float quantity = -downSlope.y;
			quantity = quantity > 0.7f? quantity +0.7f -quantity: quantity;
			Vector3 downForce = downSlope *quantity;
			rb.velocity = rb.velocity +downForce;
		}

		// handle the maximum acceleration
		Vector3 force = desiredVel -rb.velocity;
		float maxAccel = actualAcceleration;
		if (force.magnitude > maxAccel) {
			force.Normalize();
			force *= maxAccel;
		}
		if (force.magnitude != float.NaN)
		rb.AddForce(force, ForceMode.VelocityChange);

		// play footstep sounds
		if (!isGrounded())
			return;

		float currentSpeed = rb.velocity.sqrMagnitude;
		if (nextFootstep <= Time.fixedTime && currentSpeed > 0.1f) {
			if (nextFootstep != 0) {
				float volume = 0.8f -(0.8f / (1 + currentSpeed /100));
				playFootstep(volume);
				LabelHandle audioLabel = new LabelHandle(transform.position, "footsteps");
				audioLabel.addTag(new Tag(TagEnum.Sound, volume, audioLabel));
				audioLabel.addTag(new Tag(TagEnum.Threat, 5f, audioLabel));
				AudioEvent footStepsEvent = new AudioEvent(transform.position, audioLabel, transform.position);
				footStepsEvent.broadcast(volume);
				player.inventory.doStep(volume);
			}
			nextFootstep = Time.fixedTime + minimumFoostepOccurence / (1 + currentSpeed * foostepSpeedScale);
		}
		if (desiredVel.sqrMagnitude == 0 && currentSpeed < 1f && nextFootstep != 0) {
			float volume = 0.1f * currentSpeed;
			playFootstep(volume);
			nextFootstep = 0;
		}


		// update sprinting
		if (desiredVel.sqrMagnitude > 0.1f && sprinting) {
			if (player.oxygen < oxygenStopSprint)
				player.oxygen -= player.oxygenRecoveryRate *Time.deltaTime;
			else if (crouching)
				sprinting = false;
			else
				player.oxygen -= oxygenSprintUsage * Time.deltaTime;
		}
	}

	public float minDist = .1f;
	private Vector3 getDesiredVel() {
		Vector3 desiredVel;
		if (autoMove) {
			Vector3 dir = this.navPoint -transform.position;
			dir.y = 0;
			if (dir.magnitude < minDist) {
				desiredVel = Vector3.zero;
				autoMove = false;
			} else {
				desiredVel = dir.normalized * walkSpeedf;
			}
		} else {
			desiredVel = new Vector3(rightSpeed, 0, forwardSpeed);
			desiredVel = player.cam.transform.TransformDirection(desiredVel);

			// prevent player from going faster by moving diagonally
			desiredVel = Vector3.ClampMagnitude(desiredVel, Mathf.Max(Mathf.Abs(forwardSpeed), Mathf.Abs(rightSpeed)));
		}
		return desiredVel;
	}

	protected float calculateSprintMultiplier() {
		if (player.oxygen < oxygenStopSprint)
			return (sprintMult -1) *(player.oxygenRecoveryRate /oxygenSprintUsage) + 1;
		return sprintMult;
	}

	private void playFootstep(float volume) {
		AudioClip clip = footsteps[Random.Range(0, footsteps.Length)];
		footstepEmitter.pitch = 0.9f +0.2f *Random.value;
		footstepEmitter.PlayOneShot(clip, volume);
	}

	void OnCollisionEnter(Collision collisionInfo) {
		++collisionCount;
		doFallDamage(collisionInfo);
	}

	void OnCollisionStay(Collision collisionInfo) {
		doFallDamage(collisionInfo);
	}

	void OnCollisionExit(Collision collisionInfo) {
		--collisionCount;

	}

	protected void doFallDamage(Collision collisionInfo) {
		lastRelativeVelocity = collisionInfo.relativeVelocity;
		lastRelativeVelocity.y = Mathf.Clamp(lastRelativeVelocity.y, -walkSpeedf, walkSpeedf); // to represent that the player can't run on walls.
		lastRelativeVelocity.z = Mathf.Clamp(lastRelativeVelocity.z, -walkSpeedf *sprintMult *2, walkSpeedf * sprintMult *2); // to represent that the player can't run on walls.
		lastRelativeVelocity.x = Mathf.Clamp(lastRelativeVelocity.x, -walkSpeedf * sprintMult *2, walkSpeedf * sprintMult *2); // to represent that the player can't run on walls.
		Vector3 relativeVelocity = collisionInfo.relativeVelocity - lastRelativeVelocity;
		float relativeSpeed = Mathf.Max(relativeVelocity.magnitude, 0.01f);
		float impulse = Mathf.Max(collisionInfo.impulse.magnitude, 0.01f);
		float collisionSpeed = Mathf.Sqrt(relativeSpeed * impulse * (1 - Mathf.Abs(Vector3.Dot(relativeVelocity / relativeSpeed, collisionInfo.impulse / impulse)))) + impulse;
		if (collisionSpeed > fallHurtSpeed / 8f) {
			//print("Speed: " + relativeSpeed +", Impulse: " + impulse +", Combined: " + collisionSpeed);
			pastForces.Add(collisionSpeed);

			float forceSum = 0;
			for (int i = 0; i < pastForces.Count; ++i) {
				forceSum += pastForces[i];
				//pastForces[i] *= 0.9f;
			}
			float averageForce = forceSum + collisionSpeed;
			if (averageForce > 5)
				//print("Force: " + averageForce);
				if (averageForce > fallHurtSpeed && collisionSpeed > fallHurtSpeed / 2f) {
					float damage = (averageForce - fallHurtSpeed) / (fallDeathSpeed - fallHurtSpeed);
					player.health.hurt(damage * player.health.maxSuffering);
					//pastForces[pastForces.Count - 1] *= 0.5f;
				}
		}
	}

	public override void setForward(float percent) {
		if (percent > 0)
			forwardSpeed = percent *walkSpeedf;
		else
			forwardSpeed = percent *walkSpeedb;
	}

	public override void setRight(float percent) {
		if (percent > 0)
			rightSpeed = percent *walkSpeedr;
		else
			rightSpeed = percent *walkSpeedl;
	}

	public override void setSprinting(bool sprint) {
		sprinting = sprint;
	}

	public override void setCrouching(bool crouch) {
		crouching = crouch;
	}

	public override void jump() {
		if (!canMove) return;
		if (freeFallDelay == 0 && !isGrounded())
			return;
		freeFallDelay = 0;
		Vector3 upSpeed = new Vector3(0, jumpSpeed, 0);
		rb.AddForce(upSpeed, ForceMode.VelocityChange);
	}

	private void setColliderHeight(float h) {
		if (h < col.radius * 2) h = col.radius * 2;
		col.center = col.center + new Vector3(0, (col.height - h) / 2, 0);
		col.height = h;
	}

	public bool isGrounded() {
		return (floors.Count > 0 && highestFlatFloor >= 0);
	}

	public override void lockMovement() {
		canMove = false;
	}

	public override void unlockMovement() {
		canMove = true;
	}

	public override void moveToPoint(Vector3 point) {
		navPoint = point;
		autoMove = true;
	}

	public override bool isAutoMoving() {
		return autoMove;
	}

	protected Vector3 calculateAcceleration(Vector3 desiredVelocity) {
		return Vector3.one;
	}

	protected void detectFloor() {
		floors.Clear();
		float startOffset = -0.5f;

		//RaycastHit hit;
		//while(Physics.SphereCast(transform.position -Vector3.up *startOffset, 0.35f, -Vector3.up, out hit, standingElevation -startOffset +0.25f)) {
		//	hit.distance += startOffset;
		//	startOffset = hit.distance +standingElevation *0.01f;
		//	if (hit.collider.gameObject != gameObject)
		//		floors.Add(new FloorData(hit.distance, hit.normal, hit.collider));
		//}

		RaycastHit[] hits = Physics.SphereCastAll(transform.position -Vector3.up *startOffset, 0.35f, -Vector3.up, standingElevation -startOffset);
		foreach (RaycastHit hit in hits) {
			if (hit.collider.gameObject != gameObject) {
				floors.Add(new FloorData(
					hit.distance +startOffset,
					hit.normal,
					hit.collider));
			}
		}
		floors.Sort();

		// determine highest sufficiently flat floor
		highestFlatFloor = -1;
		flatestFloor = 0;
		int i = 0;
		foreach (FloorData floor in floors) {
			if (floor.normal.y > 0.1f && highestFlatFloor == -1)
				highestFlatFloor = i;
			if (floor.normal.y > floors[flatestFloor].normal.y)
				flatestFloor = i;
			++i;
		}
	}



	/////////////////////
	//  Climbing Code  //
	/////////////////////
	protected Vector3 getCapsuleStart() {
		return transform.position +Vector3.up *3;
	}

	protected Vector3 getCapsuleEnd() {
		return getCapsuleStart() + new Vector3(0, 2f -0.8f, 0);
	}

	protected Vector3 getDirection() {
		Vector3 ledgeDirection = player.cam.transform.forward;
		ledgeDirection.y = 0;
		return ledgeDirection.normalized;
	}

#if UNITY_EDITOR

	//public void OnGUI() {
	//	GUI.Label(new Rect(100, 10, 100, 20), "Grounded: " +isGrounded());
	//	GUI.Label(new Rect(100, 30, 100, 20), "Floor Count: " +floors.Count);
	//	GUI.Label(new Rect(100, 50, 100, 20), "Speed: " +rb.velocity.magnitude.ToString());
	//}

	//public void OnDrawGizmosSelected() {
	//	if (!Application.isPlaying)
	//		return;

	//	RaycastHit hitInfo;
	//	bool ledgeExists = Physics.SphereCast(getCapsuleStart() +getDirection() *1f, 0.4f, -Vector3.up, out hitInfo, 2.5f);

	//	float dropDistance = ledgeExists ? hitInfo.distance -0.05f : 0;
	//	bool ledgeFree = false;
	//	if (ledgeExists)
	//		ledgeFree = !Physics.CapsuleCast(getCapsuleStart() -Vector3.up *dropDistance, getCapsuleEnd() -Vector3.up *dropDistance, 0.4f, getDirection(), 1);

	//	Gizmos.color = ledgeFree ? Color.blue : Color.red;
	//	Gizmos.DrawWireSphere(getCapsuleStart() +getDirection() *1f -Vector3.up *dropDistance, 0.4f);
	//	Gizmos.DrawWireSphere(getCapsuleEnd() +getDirection() *1f -Vector3.up *dropDistance, 0.4f);
	//}
#endif

	protected class FloorData: System.IComparable<FloorData> {
		public float elevation;
		public Vector3 normal;
		public Collider collider;

		public FloorData(float elevation, Vector3 normal, Collider collider) {
			this.elevation = elevation;
			this.normal = normal;
			this.collider = collider;
		}

		public int CompareTo(FloorData other) {
			return elevation.CompareTo(other.elevation);
		}
	}
}
