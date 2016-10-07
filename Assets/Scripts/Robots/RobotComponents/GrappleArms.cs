using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(LineRenderer))]
public class GrappleArms : AbstractArms {

	public static Vector3 HOLD_POSITION = new Vector3(0, -0.25f, .85f);
	public static Vector3 GRAPPLE_POSITION = new Vector3(0, -0.25f, 0.25f);

	public float damagePerSecond = 1f;
	public float range = 10;
	public float checkTargetRate = 0.1f;
	public float reelSpeed = 5;
	public float reelForce = 1;
	
	public EffectSpec hitEffect;
	public EffectSpec retractedEffect;
	public EffectSpec releasedEffect;
	public EffectSpec dropEffect;
	public AudioSource windSound;
	public AudioSource electrocuteSound;

	public Vector3 throwForce = new Vector3(0, 150, 300);

	private bool targetReeledIn = false;

	private LineRenderer lr;
	protected LineRenderer cable { get {
			if (lr == null) {
				lr = GetComponent<LineRenderer>();
			}
			return lr;
		}
	}

	void FixedUpdate() {
		if (powerSource == null || !powerSource.hasPower(Time.deltaTime)) {
			releaseTarget();
		} else {
			if (targetCaptured()) {
				if (captured.GetComponent<Player>() != null && captured.GetComponent<Player>().frozen) {
					releaseTarget();
					return;
				}
				if (targetReeledIn) {
					captured.sendTrigger(this.gameObject, new DamageTrigger(damagePerSecond * Time.deltaTime));
				} else {
					reelInTarget();
				}
				updateChain();
			}
		}
	}

	[Server]
	public override void releaseCaptured() {
		if (captured != null) {
			captured.clearTag(TagEnum.Grabbed);
			getController().enqueueMessage(new RobotMessage(RobotMessage.MessageType.ACTION, RELEASED_CAPTURED_MESSAGE, captured.labelHandle, captured.transform.position, null));

            NetworkIdentity netId = captured.GetComponent<NetworkIdentity>();
            if (targetReeledIn) {
				detachRigidbody(captured);
                if (netId != null)
                    RpcDetachTarget(netId.netId);
            } else {
				releaseRigidbody(captured);
                if (netId != null)
                    RpcReleaseTarget(netId.netId);
            }


			captured = null;
			cable.enabled = false;
		}
	}

	[Server]
	public void electrifyTarget() {
		if (captured != null && targetReeledIn) {
			electrocuteSound.Play();
			captured.sendTrigger(this.gameObject, new ElectricShock());
		}
	}

	public override void setTarget(Label target) {
		base.setTarget(target);
		CancelInvoke("tryCaptureTarget");
		InvokeRepeating("tryCaptureTarget", checkTargetRate *1.23f, checkTargetRate);
    }

	public override void releaseTarget() {
		base.releaseTarget();
		CancelInvoke("tryCaptureTarget");
	}
    
	protected void tryCaptureTarget() {
		if (!targetCaptured() && target != null) {
			System.Nullable<RaycastHit> hit = getHitLocation(target);
			if (hit != null) {
				hitEffect.spawn(hit.Value.point, hit.Value.normal);
				captureTarget(target);
			}
		}
	}

	protected void reelInTarget() {
		Vector3 holdPos = transform.TransformPoint(HOLD_POSITION);
		Vector3 diff = holdPos - captured.transform.position;

		// check if grabbed
		if (diff.sqrMagnitude < 1f) {
			attachRigidbody(captured);
			NetworkIdentity netId = captured.GetComponent<NetworkIdentity>();
			if (netId != null)
				RpcAttachTarget(netId.netId);
			return;
		}

		// check if obstructed
		System.Nullable<RaycastHit> hit = getHitLocation(captured);
        if (hit == null) {
			releaseCaptured();
			// TODO: play sound
			return;
		}

		// reel in target
		Vector3 reelDir = diff.normalized;
		Rigidbody targetRb = captured.GetComponent<Rigidbody>();
		float speedComponent = Mathf.Min(reelSpeed, Vector3.Dot(reelDir, targetRb.velocity)
			-Vector3.Dot(reelDir, getController().GetComponent<Rigidbody>().velocity));
        if (speedComponent < reelSpeed) {
			targetRb.velocity += reelDir *Mathf.Min(reelSpeed - speedComponent, reelForce /targetRb.mass *Time.fixedDeltaTime);
		}
	}

	private System.Nullable<RaycastHit> getHitLocation(Label target) {
		Vector3 position = transform.TransformPoint(HOLD_POSITION);
		Vector3 diff = target.transform.position - position;

		// check distance
		if (diff.sqrMagnitude > range * range) {
			return null;
		}

		// check for obstructions
		RaycastHit hit;
		if (Physics.Raycast(position, diff, out hit, range)) {
			if (hit.transform == target.transform) {
				return hit;
			} else {
				return null;
			}
		}
		hit = new RaycastHit();
		hit.point = target.transform.position;
		hit.normal = -diff.normalized;
        return hit;
	}

	[Server]
	protected void captureTarget(Label proposedTarget) {
		if (captured == null) {
            NetworkIdentity netId = proposedTarget.GetComponent<NetworkIdentity>();
            captureRigidBody(proposedTarget);
            RpcCaptureTarget(netId.netId);
			getController().enqueueMessage(new RobotMessage(
			RobotMessage.MessageType.ACTION, TARGET_CAPTURED_MESSAGE,
			proposedTarget.labelHandle, proposedTarget.transform.position, null));
			captured.setTag(new Tag(TagEnum.Grabbed, 0));
		}
	}


    [ClientRpc]
    protected void RpcCaptureTarget(NetworkInstanceId netId) {
        captureRigidBody(ClientScene.FindLocalObject(netId).GetComponent<Label>());
    }


	[ClientRpc]
	protected void RpcReleaseTarget(NetworkInstanceId netId) {
	    releaseRigidbody(ClientScene.FindLocalObject(netId).GetComponent<Label>());
	}

    [ClientRpc]
    protected void RpcDetachTarget(NetworkInstanceId netId) {
        detachRigidbody(ClientScene.FindLocalObject(netId).GetComponent<Label>());
    }

    [ClientRpc]
    protected void RpcAttachTarget(NetworkInstanceId netId) {
        attachRigidbody(ClientScene.FindLocalObject(netId).GetComponent<Label>());
    }

    protected void updateChain() {
        cable.SetPositions(new Vector3[] {
            transform.TransformPoint(GRAPPLE_POSITION),
            captured.transform.position
        });
    }

    protected void captureRigidBody(Label proposedTarget) {
        captured = proposedTarget;
        targetReeledIn = false;
        updateChain();
        cable.enabled = true;
        windSound.Play();
    }

    protected void releaseRigidbody(Label target) {
        captured = null;
        Rigidbody rigidbody = target.GetComponent<Rigidbody>();
		if (rigidbody != null) {
			rigidbody.isKinematic = false;
			rigidbody.useGravity = true;
		}
		windSound.Stop();
		releasedEffect.spawn(target.transform.position);
	}

	protected void detachRigidbody(Label target) {
        captured = null;
		Rigidbody rigidbody = target.GetComponent<Rigidbody>();
		if (rigidbody != null) {
			rigidbody.isKinematic = false;
			rigidbody.useGravity = true;
			rigidbody.AddForce(transform.forward * throwForce.z + transform.up * throwForce.y);
		}
		target.transform.parent = null;
		electrocuteSound.Stop();
		dropEffect.spawn(target.transform.position);
	}

	protected void attachRigidbody(Label proposedTarget) {
        captured = proposedTarget;
		Rigidbody rigidbody = proposedTarget.GetComponent<Rigidbody>();
		if (rigidbody != null) {
			rigidbody.isKinematic = true;
			rigidbody.useGravity = false;
			rigidbody.velocity = new Vector3(0, 0, 0);
		}

        proposedTarget.transform.parent = transform;
        proposedTarget.transform.localPosition = HOLD_POSITION;
		targetReeledIn = true;
		windSound.Stop();
		retractedEffect.spawn(transform.position);
		electrocuteSound.Play();
	}
}
