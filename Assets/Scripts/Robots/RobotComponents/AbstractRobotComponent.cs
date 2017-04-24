using UnityEngine;
using UnityEngine.Networking;

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
		DismantleEffect.dismantle(transform, lingerTime, isServer, Vector3.zero);
	}
}
