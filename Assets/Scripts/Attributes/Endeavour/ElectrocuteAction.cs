using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ElectrocuteAction : Endeavour {

	public ElectrocuteAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tagMap)
		: base(factory, controller, goals, tagMap) {
		this.name = "electrocute";
	}

	public override System.Type[] getRequiredComponents() {
		return new System.Type[] { typeof(AbstractArms) };
	}

	public override bool isStale() {
		return false;
	}

	protected override void onExecute() {
		ZappyArms arms = controller.getRobotComponent<ZappyArms>();
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

	public override TagEnum getPrimaryTagType() {
		return TagEnum.None;
	}
}
