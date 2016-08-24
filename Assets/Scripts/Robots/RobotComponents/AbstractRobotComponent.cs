using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public abstract class AbstractRobotComponent : NetworkBehaviour {

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
	protected bool isOccupied = false;

	public RobotController getController() {
		if (roboController == null) {
			roboController = GetComponentInParent<RobotController>();
			if (roboController == null)
				Debug.LogWarning("Robot component '" + name + "' is not attached to a robot controller!");
		}
		return roboController;
	}

	public bool isAvailable() {
		return isOccupied;
	}

	public void setAvailability(bool availability) {
		isOccupied = !availability;
	}

	public virtual System.Type getComponentArchetype() {
		return this.GetType();
	}

    [Server]
    public virtual void dismantle() {
        dismantle(transform);
        RpcDismantle();
    }

    [ClientRpc]
    protected void RpcDismantle() {
        dismantle(transform);
    }

    protected void dismantle(Transform trans) {
        trans.parent = null;
        trans.gameObject.hideFlags |= HideFlags.HideInHierarchy;
        while (trans.childCount > 0)
            dismantle(trans.GetChild(0));
        Collider [] cols = trans.GetComponents<Collider>();

        bool hasCollider = false;
        foreach (Collider col in cols) {

            if (col.isTrigger) {
                continue;
            } else {
                hasCollider = true;
                //foreach (MonoBehaviour script in trans.GetComponents<MonoBehaviour>()) {
                //    if (script as NetworkIdentity == null) {
                //        Destroy(script);
                //    }
                //}
                if (col as MeshCollider != null)
                    ((MeshCollider)col).convex = true;
                col.enabled = true;
            }
        }

        if (hasCollider) {
            if (isServer) {
                useAsTemporaryDebris(trans);
            } else {
                convertToDebris(trans);
            }
        } else {
            Destroy(trans.gameObject);
        }
    }

    [Server]
    protected static void useAsTemporaryDebris(Transform trans) {
        const float lingerTime = 30;
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
