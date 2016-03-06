using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[AddComponentMenu("Scripts/Doors/Door Trigger")]
public class DoorTrigger : NetworkBehaviour {

	private AutoDoor control = null;
	private int robotCount = 0;

	void Awake() {
		control = GetComponentInParent<AutoDoor> ();
	}

	[ServerCallback]
	void OnTriggerEnter(Collider collision) {
		RobotController controller = collision.GetComponent<RobotController> ();
		if (controller != null) {
			robotCount++;
			control.open();
		}
	}

	[ServerCallback]
	void OnTriggerExit(Collider other) {
		RobotController controller = other.GetComponent<RobotController> ();
		if(controller != null) {
			--robotCount;
			if(robotCount == 0) {
				control.close();
			}
		}
	}
}
