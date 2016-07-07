using UnityEngine;
using System.Collections;

[System.Serializable]
public class ActivateSpawner : EndeavourFactory {
	
	public override Endeavour constructEndeavour(RobotController controller) {
		if (parent == null) {
			return null;
		}
		return new ActivateSpawnerAction(this, controller, goals, parent.labelHandle, parent.GetComponent<RobotSpawner>());
	}
}
