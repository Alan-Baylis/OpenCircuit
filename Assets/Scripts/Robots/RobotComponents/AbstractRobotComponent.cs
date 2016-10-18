using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public abstract class AbstractRobotComponent : NetworkBehaviour {

	public const float lingerTime = 30;

	private bool isAttached;

	private AbstractPowerSource myPowerSource;
	public AbstractPowerSource powerSource { get {
			if (myPowerSource == null) {
				myPowerSource = getController().GetComponentInChildren<AbstractPowerSource>();
                if (myPowerSource == null)
					Debug.LogWarning("Robot component '" + name + "' has no power source!");
			}
			return myPowerSource;
		} }
	private RobotController roboController;

	public RobotController getController() {
		if (roboController == null) {
			roboController = GetComponentInParent<RobotController>();
			if (roboController == null)
				Debug.LogWarning("Robot component '" + name + "' is not attached to a robot controller!");
		}
		return roboController;
	}

    public void attachToController(RobotController controller) {
        roboController = controller;
        roboController.attachRobotComponent(this);
        isAttached = true;
    }

    public void detachFromController() {
        roboController.detachRobotComponent(this);
        roboController = null;
        isAttached = false;
    }

	public virtual void release() {
	}

	public virtual System.Type getComponentArchetype() {
		return this.GetType();
	}

    [Server]
    public void dismantle() {
		RpcDismantle();
		dismantleEffect();
		isAttached = false;
    }

    [ClientRpc]
    protected void RpcDismantle() {
        dismantleEffect();
        isAttached = false;
    }

    void OnDestroy() {
        if (isAttached) {
            detachFromController();
        }
    }

    public bool isComponentAttached() {
        return isAttached;
    }

	protected virtual void dismantleEffect() {
		dismantleEffect(transform);
	}

    protected void dismantleEffect(Transform trans) {
        trans.parent = null;
        trans.gameObject.hideFlags |= HideFlags.HideInHierarchy;
        while (trans.childCount > 0)
            dismantleEffect(trans.GetChild(0));
        Collider [] cols = trans.GetComponents<Collider>();

        bool hasCollider = false;
        foreach (Collider col in cols) {

            if (col.isTrigger) {
                continue;
            } else {
                hasCollider = true;
                if (col as MeshCollider != null)
                    ((MeshCollider)col).convex = true;
                col.enabled = true;
            }
        }
		
        if (isServer || trans.GetComponent<NetworkIdentity>() == null) {
			if (hasCollider) {
				useAsTemporaryDebris(trans);
			} else {
				Destroy(trans.gameObject, lingerTime);
			}
		} else if (hasCollider) {
			convertToDebris(trans);
        }
    }

    protected static void useAsTemporaryDebris(Transform trans) {
        convertToDebris(trans);
        Destroy(trans.gameObject, lingerTime);
    }

    protected static void convertToDebris(Transform trans) {
        const float maxForce = 200;
        Rigidbody rb = trans.GetComponent<Rigidbody>();
        if (rb == null)
            rb = trans.gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.AddForce(randomRange(Vector3.one * -maxForce, Vector3.one * maxForce));
    }

    protected static Vector3 randomRange(Vector3 min, Vector3 max) {
        return new Vector3(
            UnityEngine.Random.Range(min.x, max.x),
            UnityEngine.Random.Range(min.y, max.y),
            UnityEngine.Random.Range(min.z, max.z));
    }
}
