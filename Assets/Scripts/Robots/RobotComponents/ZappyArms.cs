using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Scripts/Robot/Zappy Arms")]
public class ZappyArms : AbstractArms {

	public static Vector3 HOLD_POSITION = new Vector3(0, -0.25f, .85f);

	public float damagePerSecond = 1f;

	public AudioClip pickUp;
	public AudioClip drop;
	public AudioClip zap;

	public Vector3 throwForce = new Vector3(0, 150, 300);

	private AudioSource footstepEmitter;

	void Start() {
		footstepEmitter = gameObject.AddComponent<AudioSource>();
		footstepEmitter.enabled = true;
		footstepEmitter.loop = false;
	}

    [ServerCallback]
	void Update() {
		BoxCollider collider = GetComponent<BoxCollider>();
        if (powerSource == null || !powerSource.hasPower(Time.deltaTime)) {
			collider.enabled = false;
			releaseTarget();
		} else {
			collider.enabled = true;
			if(targetCaptured()) {
                if (captured.GetComponent<Player>() != null && captured.GetComponent<Player>().frozen) {
                    releaseTarget();
                    return;
                }
				captured.sendTrigger(this.gameObject, new DamageTrigger(damagePerSecond * Time.deltaTime));
				if(!footstepEmitter.isPlaying) {
					footstepEmitter.PlayOneShot(zap);
				}
			}
		}
	}
	
    [ServerCallback]
	void OnTriggerEnter(Collider collision) {
		if(!targetCaptured()) {
			Label proposedTarget = collision.gameObject.GetComponent<Label>();
			if(proposedTarget != null && proposedTarget == target) {
				captureTarget(proposedTarget);
			}
		}
	}
	
	[Server]
	public override void releaseCaptured() {
		if (captured != null) {
			captured.clearTag(TagEnum.Grabbed);
			getController().enqueueMessage(new RobotMessage(RobotMessage.MessageType.ACTION, RELEASED_CAPTURED_MESSAGE, captured.labelHandle, captured.transform.position, null));
			footstepEmitter.PlayOneShot(drop, 1);

			dropRigidbody(captured.gameObject);
			NetworkIdentity netId = captured.GetComponent<NetworkIdentity>();
			if (netId != null)
				RpcReleaseTarget(netId.netId);

			captured = null;
		}
	}

	[Server]
	public void electrifyTarget() {
		if(captured != null) {
			footstepEmitter.PlayOneShot(zap, 1);
			captured.sendTrigger(this.gameObject, new ElectricShock());
		}
	}



	[Server]
	protected void captureTarget(Label proposedTarget) {
		if (captured == null) {
			captured = proposedTarget;
			getController().enqueueMessage(new RobotMessage(
				RobotMessage.MessageType.ACTION, TARGET_CAPTURED_MESSAGE,
				proposedTarget.labelHandle, proposedTarget.transform.position, null));
			captured.setTag(new Tag(TagEnum.Grabbed, 0, proposedTarget.labelHandle));
			attachRigidbody(proposedTarget.gameObject);
			NetworkIdentity netId = proposedTarget.GetComponent<NetworkIdentity>();
			if (netId != null)
				RpcCaptureTarget(netId.netId);
		}
	}
	
	[ClientRpc]
    protected void RpcReleaseTarget(NetworkInstanceId netId) {
        dropRigidbody(ClientScene.FindLocalObject(netId));
    }

    [ClientRpc]
    protected void RpcCaptureTarget(NetworkInstanceId netId) {
        attachRigidbody(ClientScene.FindLocalObject(netId));
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
}
