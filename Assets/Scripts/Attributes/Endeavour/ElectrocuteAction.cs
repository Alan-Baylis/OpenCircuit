using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ElectrocuteAction : Endeavour {

	public ElectrocuteAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Label target)
		: base(factory, controller, goals, target.labelHandle) {
		this.name = "electrocute";
	}

	public override System.Type[] getRequiredComponents() {
		return new System.Type[] { typeof(HoverJet) };
	}

	public override bool isStale() {
		return false;
	}

	protected override void onExecute() {
		RobotArms arms = controller.GetComponentInChildren<RobotArms>();
		if(arms != null) {
			arms.electrifyTarget();
		}
	}

	public override bool canExecute() {
		return true;
	}

	public override bool singleExecutor() {
		return true;
	}

	protected override float getCost() {
		return 0;
	}

}
