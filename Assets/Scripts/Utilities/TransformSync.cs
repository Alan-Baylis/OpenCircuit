using UnityEngine;
using UnityEngine.Networking;

public class TransformSync : NetworkBehaviour {

	public enum SyncMode {
		POSITION,
		ROTATION,
		BOTH,
		VELOCITY
	}

	public SyncMode mode = SyncMode.BOTH;
	public float interpolationRate = .2f;
	public int networkChannel = 1;
	[SerializeField, ReadOnly]
    private Transform syncTarget;

	[SyncVar]
	protected Vector3 serverPosition;
	[SyncVar]
	protected Quaternion serverRotation;
	[SyncVar]
	protected Vector3 serverVelocity;
	[SyncVar]
	protected Vector3 serverAngularVelocity;

	private new Comp<Rigidbody> rigidbody;
	private Comp<UnityEngine.AI.NavMeshAgent> navAgent;

	public void Awake() {
		rigidbody.init(getTransform());
		navAgent.init(getTransform());
		serverPosition = getTransform().position;
		serverRotation = getTransform().rotation;
	}

	public void FixedUpdate() {
        if (mode != SyncMode.ROTATION)
            syncPosition();
        if (mode != SyncMode.POSITION)
            syncRotation();
		if (mode == SyncMode.VELOCITY)
			syncVelocity();
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

	protected virtual void syncVelocity() {
		if (rigidbody.get != null) {
			if (isServer) {
				if (navAgent.get == null) {
					serverVelocity = rigidbody.get.velocity;
				} else {
					serverVelocity = getTransform().InverseTransformVector(navAgent.get.velocity);
				}
				serverAngularVelocity = rigidbody.get.angularVelocity;
			} else {
				Vector3 diff = serverVelocity - rigidbody.get.velocity;
				rigidbody.get.velocity += diff * interpolationRate;
				diff = serverAngularVelocity - rigidbody.get.angularVelocity;
				rigidbody.get.angularVelocity += diff * interpolationRate;
				//rigidbody.get.velocity = Vector3.Lerp(rigidbody.get.velocity, serverVelocity, interpolationRate);
				//rigidbody.get.angularVelocity = Vector3.Lerp(rigidbody.get.angularVelocity, serverAngularVelocity, interpolationRate);
			}
		}
	}
	
	private struct Comp<T> where T : Component {
		public T get { get {
				if (cache == null) {
					cache = owner.GetComponent<T>();
				}
				return cache;
			}
		}
		private T cache;

		private Component owner;

		public void init(Component owner) {
			this.owner = owner;
		}
	}
}
