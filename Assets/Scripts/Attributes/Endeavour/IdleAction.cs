using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class IdleAction : Endeavour {

    public IdleAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Label target) : base(factory, controller, goals, target.labelHandle) {

    }

	protected override void onExecute() {
	}

    public override bool isStale() {
      return false;
    }

    protected override float getCost() {
        return 0f;
	}

	public override System.Type[] getRequiredComponents() {
		return new System.Type[] {};
	}

	public override bool canExecute() {
		return true;
	}

	public override bool singleExecutor() {
		return false;
	}
}
