using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class TransformSync : NetworkBehaviour {

	public enum SyncMode {
		POSITION,
		ROTATION,
		BOTH
	}

	public SyncMode mode = SyncMode.BOTH;
	public float interpolationRate;
	public int networkChannel = 1;

	[SyncVar]
	protected Vector3 serverPosition;
	[SyncVar]
	protected Quaternion serverRotation;

	public void Awake() {
		serverPosition = transform.position;
	}

	public void FixedUpdate() {
		if (isServer) {
			if (mode != SyncMode.ROTATION)
				serverPosition = transform.position;
			if (mode != SyncMode.POSITION)
				serverRotation = transform.rotation;
		} else {
			if (mode != SyncMode.ROTATION) {
				Vector3 diff = serverPosition -transform.position;
				transform.position += diff *interpolationRate;
			}
			if (mode != SyncMode.POSITION) {
				transform.rotation = Quaternion.Lerp(transform.rotation, serverRotation, interpolationRate);
			}
		}
	}

	public override int GetNetworkChannel() {
		return networkChannel;
	}
}
