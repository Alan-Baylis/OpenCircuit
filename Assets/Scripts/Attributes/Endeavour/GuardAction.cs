using UnityEngine;
using System.Collections.Generic;
using System;

public class GuardAction : Endeavour {

	Tag guardLocation;

	public GuardAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tagMap) : base(factory, controller, goals, tagMap){
		this.guardLocation = getTagOfType<Tag>(TagEnum.GuardPoint);
		this.name = "guard";
	}

	protected override void onExecute() {
		HoverJet jet = controller.GetComponentInChildren<HoverJet>();
		if(jet != null ) {
			jet.setTarget(guardLocation.getLabelHandle(), true, true);
		}
	}

	public override bool isStale() {
		return false;
	}

	public override System.Type[] getRequiredComponents() {
		return new System.Type[] { typeof(HoverJet) };
	}

	public override bool canExecute() {
		HoverJet jet = controller.GetComponentInChildren<HoverJet>();
		if(this.active && !jet.hasTarget() && !jet.hasReachedTarget(guardLocation.getLabelHandle())) {
			this.active = false;
		}
		return true;
	}

	public override bool singleExecutor() {
		return false;
	}

	protected override float getCost() {
		HoverJet jet = controller.GetComponentInChildren<HoverJet>();
		if(jet != null) {
			return jet.calculatePathCost(guardLocation.getLabelHandle().label);
		}
		return 0;
	}

	public override TagEnum getPrimaryTagType() {
		return TagEnum.GuardPoint;
	}
}
