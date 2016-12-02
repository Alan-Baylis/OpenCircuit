using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class IdleAction : Endeavour {

    public IdleAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tags) : base(factory, controller, goals, tags) {

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

	public override TagEnum getPrimaryTagType() {
		return TagEnum.None;
	}
}
