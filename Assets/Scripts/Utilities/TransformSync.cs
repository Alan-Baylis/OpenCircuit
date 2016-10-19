using UnityEngine;
using UnityEngine.Networking;

public class TransformSync : NetworkBehaviour {

	public enum SyncMode {
		POSITION,
		ROTATION,
		BOTH
	}

	public SyncMode mode = SyncMode.BOTH;
	public float interpolationRate = .2f;
	public int networkChannel = 1;
    public Transform syncTarget;

	[SyncVar]
	protected Vector3 serverPosition;
	[SyncVar]
	protected Quaternion serverRotation;

	public void Awake() {
		serverPosition = getTransform().position;
	}

	public void FixedUpdate() {
        if (mode != SyncMode.ROTATION)
            syncPosition();
        if (mode != SyncMode.POSITION)
            syncRotation();
	}

	public override int GetNetworkChannel() {
		return networkChannel;
	}

    private Transform getTransform() {
        if (syncTarget != null) {
            return syncTarget;
        }
        return transform;
    }

    protected virtual void syncRotation() {
        if (isServer) {
            serverRotation = getTransform().rotation;
        } else {
            getTransform().rotation = Quaternion.Lerp(getTransform().rotation, serverRotation, interpolationRate);
        }
    }

    protected virtual void syncPosition() {
        if(isServer) {
            serverPosition = getTransform().position;
        } else {
            Vector3 diff = serverPosition - getTransform().position;
            getTransform().position += diff * interpolationRate;
        }
    }
}
