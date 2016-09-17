using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public abstract class AbstractArms : AbstractRobotComponent {
	
	public const string TARGET_CAPTURED_MESSAGE = "target captured";
	public const string RELEASED_CAPTURED_MESSAGE = "released captured";

	protected Label target = null;
	protected Label captured = null;

	[Server]
	public abstract void releaseCaptured();

	[Server]
	public virtual void releaseTarget() {
		target = null;
		releaseCaptured();
	}

	[Server]
	public virtual void setTarget(Label target) {
		this.target = target;
		if (target != captured) {
			releaseCaptured();
		}
	}

	public virtual Label getTarget() {
		return target;
	}

	public virtual bool targetCaptured() {
		return target != null && target == captured;
	}
	
	public override System.Type getComponentArchetype() {
		return typeof(AbstractArms);
	}

	[ServerCallback]
	public override void release() {
		base.release();
		releaseTarget();
	}

	[ServerCallback]
    void OnDisable() {
        releaseTarget();
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.enabled = false;
    }

    [ServerCallback]
    void OnEnable() {
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.enabled = true;
    }
}
