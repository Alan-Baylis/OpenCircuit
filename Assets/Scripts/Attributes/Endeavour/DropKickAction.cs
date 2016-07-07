using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DropKickAction : Endeavour {

	LabelHandle dropPoint;

	public DropKickAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, LabelHandle dropPoint) : base(factory, controller, goals, dropPoint) {
		this.name = "dropKick";
		this.dropPoint = dropPoint;
		requiredComponents = new System.Type[] {typeof(HoverJet)};
	}

	public override bool canExecute () {
		RobotArms arms = controller.GetComponentInChildren<RobotArms> ();
		return (arms != null) && (dropPoint != null) && (arms.hasTarget());
	}

	public override void execute (){
        base.execute();
		HoverJet jet = controller.GetComponentInChildren<HoverJet> ();
		if (jet != null) {
			jet.setTarget(dropPoint, true);
			jet.setAvailability(false);
		}
	}

	public override void stopExecution(){
        base.stopExecution();
		HoverJet jet = controller.GetComponentInChildren<HoverJet> ();
		if (jet != null) {
			jet.setTarget(null, false);
			jet.setAvailability(true);
		}
	}

	public override bool isStale() {
		return dropPoint == null;
	}

	public override void onMessage(RobotMessage message) {
		if (message.Message.Equals ("target reached")) {
			RobotArms arms = controller.GetComponentInChildren<RobotArms> ();
			arms.dropTarget();
		}
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
