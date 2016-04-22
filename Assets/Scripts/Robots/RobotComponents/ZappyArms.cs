using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Scripts/Robot/Zappy Arms")]
public class ZappyArms : AbstractArms {

	public static Vector3 HOLD_POSITION = new Vector3(0, .5f, .85f);

	public float damagePerSecond = 1f;

	public AudioClip pickUp;
	public AudioClip drop;
	public AudioClip zap;

	public Vector3 throwForce = new Vector3(0, 150, 300);

	private AudioSource footstepEmitter;

	private Label target = null;
	private Label proposedTarget = null;

	private bool proposedTargetStatus = false;

	void Start() {
		footstepEmitter = gameObject.AddComponent<AudioSource>();
		footstepEmitter.enabled = true;
		footstepEmitter.loop = false;
	}

	void Update() {
		BoxCollider collider = GetComponent<BoxCollider>();
		if(powerSource == null || !powerSource.hasPower(Time.deltaTime)) {
			collider.enabled = false;
			dropTarget();
		} else {
			collider.enabled = true;
			if(hasTarget()) {
				Label label = target.GetComponent<Label>();
				label.sendTrigger(this.gameObject, new DamageTrigger(damagePerSecond * Time.deltaTime));
				if(!footstepEmitter.isPlaying) {
					footstepEmitter.PlayOneShot(zap);
				}
			}
		}
	}

	void FixedUpdate() {

		if(proposedTarget == null && target == null) {
			return;
		}
		if(proposedTarget != null && !proposedTargetStatus) {
			proposedTarget = null;
		}

		if(target != null) {
			if(Vector3.Distance(target.transform.localPosition, HOLD_POSITION) > .0001f) {
				target = null;
			}
		}

		proposedTargetStatus = false;

	}

	void OnTriggerEnter(Collider collision) {
		if(target == null) {
			proposedTarget = collision.gameObject.GetComponent<Label>();
			proposedTargetStatus = true;
			if(proposedTarget != null && proposedTarget.hasTag(TagEnum.GrabTarget)) {

				// footstepEmitter.PlayOneShot(pickUp, 1);
				roboController.addEndeavour(new HoldAction(roboController, proposedTarget, proposedTarget.labelHandle));
			}
		}
	}

	void OnTriggerStay(Collider collision) {
		Label label = collision.gameObject.GetComponent<Label>();
		if(label == proposedTarget) {
			proposedTargetStatus = true;
		}
	}

	public override bool hasTarget() {
		return target != null;
	}

	public override void dropTarget() {
		if(target != null) {
			target.clearTag(TagEnum.Grabbed);
			roboController.enqueueMessage(new RobotMessage(RobotMessage.MessageType.ACTION, "target dropped", target.labelHandle, target.transform.position, null));
			footstepEmitter.PlayOneShot(drop, 1);

			dropRigidbody(target.gameObject);
			NetworkIdentity netId = target.GetComponent<NetworkIdentity>();
			if (netId != null)
				RpcDropTarget(netId.netId);

			target = null;
		}
	}

	[ClientRpc]
	protected void RpcDropTarget(NetworkInstanceId netId) {
		dropRigidbody(ClientScene.FindLocalObject(netId));
	}

	protected void dropRigidbody(GameObject obj) {
		Rigidbody rigidbody = obj.GetComponent<Rigidbody>();
		if (rigidbody != null) {
			rigidbody.isKinematic = false;
			rigidbody.useGravity = true;
			rigidbody.AddForce(transform.forward * throwForce.z + transform.up * throwForce.y);
		}
		obj.transform.parent = null;
	}

	public override void attachTarget(Label obj) {
		if(target == null) {
			target = obj;
			target.setTag(new Tag(TagEnum.Grabbed, 0));
			attachRigidbody(obj.gameObject);
			NetworkIdentity netId = obj.GetComponent<NetworkIdentity>();
			if (netId != null)
				RpcAttachTarget(netId.netId);
			roboController.enqueueMessage(new RobotMessage(RobotMessage.MessageType.ACTION, "target grabbed", target.labelHandle, target.transform.position, null));
			roboController.addEndeavour(new ScanAction(roboController, new List<Goal>(), target));
		}
	}

	[ClientRpc]
	protected void RpcAttachTarget(NetworkInstanceId netId) {
		attachRigidbody(ClientScene.FindLocalObject(netId));
	}

	protected void attachRigidbody(GameObject obj) {
		Rigidbody rigidbody = obj.GetComponent<Rigidbody>();
		if (rigidbody != null) {
			rigidbody.isKinematic = true;
			rigidbody.useGravity = false;
			rigidbody.velocity = new Vector3(0, 0, 0);
		}

		obj.transform.parent = transform;
		obj.transform.localPosition = HOLD_POSITION;
	}

	public void electrifyTarget() {
		if(target != null) {
			footstepEmitter.PlayOneShot(zap, 1);
			target.sendTrigger(this.gameObject, new ElectricShock());
		}
	}

	public override Label getProposedTarget() {
		return proposedTarget;
	}

	public Label getTarget() {
		return target;
	}
}
