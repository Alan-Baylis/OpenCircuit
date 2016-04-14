using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ElectrocuteAction : Endeavour {

	public ElectrocuteAction(RobotController controller, List<Goal> goals, Label target)
		: base(controller, goals, target.labelHandle) {
		this.name = "electrocute";
		requiredComponents = new System.Type[] { };

	}


	public override bool isStale() {
		return false;
	}

	public override void onMessage(RobotMessage message) {

	}

	public override void execute() {
		base.execute();
		RobotArms arms = controller.GetComponentInChildren<RobotArms>();
		if(arms != null) {
			arms.electrifyTarget();
		}
	}

	public override bool canExecute() {
		return true;
	}

	protected override float getCost() {
		return 0;
	}

}
