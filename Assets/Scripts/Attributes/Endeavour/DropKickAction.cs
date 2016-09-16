using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DropKickAction : Endeavour {

	LabelHandle dropPoint;

	public DropKickAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, LabelHandle dropPoint) : base(factory, controller, goals, dropPoint) {
		this.name = "dropKick";
		this.dropPoint = dropPoint;
	}

	public override bool canExecute () {
		RobotArms arms = controller.GetComponentInChildren<RobotArms> ();
		return (arms != null) && (dropPoint != null) && (arms.hasTarget());
	}

	protected override void onExecute() {
		HoverJet jet = controller.GetComponentInChildren<HoverJet> ();
		if (jet != null) {
			jet.setTarget(dropPoint, true);
		}
	}

	public override bool isStale() {
		return dropPoint == null;
	}

	public override void onMessage(RobotMessage message) {
		if (message.Message.Equals (HoverJet.TARGET_REACHED)) {
			RobotArms arms = controller.GetComponentInChildren<RobotArms> ();
			arms.dropTarget();
		}
	}

	public override System.Type[] getRequiredComponents() {
		return new System.Type[] { typeof(HoverJet) };
	}

	public override bool singleExecutor() {
		return true;
	}

	protected override float getCost() {
		HoverJet jet = controller.GetComponentInChildren<HoverJet> ();
		if (jet != null) {
			return jet.calculatePathCost(dropPoint.label);
		}
		return 0;
	}
}
