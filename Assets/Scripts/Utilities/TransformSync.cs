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
		if (isServer) {
			if (mode != SyncMode.ROTATION)
				serverPosition = getTransform().position;
			if (mode != SyncMode.POSITION)
				serverRotation = getTransform().rotation;
		} else {
			if (mode != SyncMode.ROTATION) {
				Vector3 diff = serverPosition - getTransform().position;
                getTransform().position += diff *interpolationRate;
			}
			if (mode != SyncMode.POSITION) {
                getTransform().rotation = Quaternion.Lerp(getTransform().rotation, serverRotation, interpolationRate);
			}
		}
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
}
