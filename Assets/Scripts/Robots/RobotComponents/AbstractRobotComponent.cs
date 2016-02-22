using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public abstract class AbstractRobotComponent : NetworkBehaviour {

	protected AbstractPowerSource powerSource;
	protected RobotController roboController;
	protected bool isOccupied = false;

	[SyncVar(hook = "applyControllerId")]
	protected NetworkInstanceId controllerId;

	// Use this for initialization
	public void Awake () {
		roboController = GetComponentInParent<RobotController> ();
        if (roboController == null) {
            Debug.LogWarning("Robot component '" + name + "' is not attached to a robot controller!");
        } else {
            powerSource = roboController.GetComponentInChildren<AbstractPowerSource>();

        }
        if (roboController == null) {
            Debug.LogWarning("Robot component '" + name + "' has no power source!");
        }

    }

	public RobotController getController() {
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
	public void setControllerId(NetworkInstanceId id) {
		this.controllerId = id;
	}

	[Client]
	protected void applyControllerId(NetworkInstanceId id) {
		this.controllerId = id;
		GameObject robotControllerObject = NetworkServer.FindLocalObject(id);
		transform.parent = robotControllerObject.transform;
	}
}
