using UnityEngine;
using System.Collections;

[System.Serializable]
public class RobotDestructionOperation : Operation {

	private static System.Type[] triggers = new System.Type[] {
		typeof(DestructTrigger),
	};

	public override System.Type[] getTriggers() {
		return triggers;
	}

	public override void perform(GameObject instigator, Trigger trig) {
		RobotController robot = parent.GetComponent<RobotController>();
		if(robot != null) {
			robot.dispose();
		}
	}

}
