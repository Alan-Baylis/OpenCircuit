using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HuntAction : Endeavour {

	private Label target;

	public HuntAction(RobotController controller, List<Goal> goals, Label target)
		: base(controller, goals, target.labelHandle) {
		this.target = target;
		this.name = "hunt";
		requiredComponents = new System.Type[] { typeof(HoverJet) };
	}

	public override bool canExecute() {
		HoverJet jet = controller.GetComponentInChildren<HoverJet>();
		ZappyArms arms = controller.GetComponentInChildren<ZappyArms>();
		return arms != null && !arms.hasTarget() && !target.hasTag(TagEnum.Grabbed) && jet != null && jet.canReach(target);
	}

	public override void execute() {
		base.execute();
		HoverJet jet = controller.GetComponentInChildren<HoverJet>();
		if(jet != null && target != null) {
			jet.pursueTarget(target.labelHandle, false);
			jet.setAvailability(false);
		}
	}

	public override void stopExecution() {
		base.stopExecution();
		HoverJet jet = controller.GetComponentInChildren<HoverJet>();
		if(jet != null) {
			jet.setTarget(null, false);
			jet.setAvailability(true);
		}
	}

	public override bool isStale() {
		return target == null || !controller.knowsTarget(target.labelHandle);
	}

	public override void onMessage(RobotMessage message) {

	}

	protected override float getCost() {
        if (target == null) {
            return float.PositiveInfinity;
        }
		HoverJet jet = controller.GetComponentInChildren<HoverJet>();
		if(jet != null) {
			return jet.calculatePathCost(target);
		}
		return 0;
	}
}
