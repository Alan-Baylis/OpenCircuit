using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public abstract class AbstractRobotComponent : NetworkBehaviour {

	private AbstractPowerSource myPowerSource;
	public AbstractPowerSource powerSource { get {
			if (myPowerSource == null) {
				myPowerSource = getController().GetComponentInChildren<AbstractPowerSource>();
				if (powerSource == null)
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
}
