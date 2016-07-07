using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PursueAction : Endeavour {

	private Label target;

	public PursueAction (EndeavourFactory factory, RobotController controller, List<Goal> goals, Label target) : base(factory, controller, goals, target.labelHandle) {
		this.target = target;
		this.name = "pursue";
		requiredComponents = new System.Type[] {typeof(HoverJet)};
	}

	public override bool canExecute () {
		HoverJet jet = controller.getRobotComponent<HoverJet> ();
        RobotArms arms = controller.GetComponentInChildren<RobotArms>();
        return arms != null && !arms.hasTarget() && !target.hasTag(TagEnum.Grabbed) && controller.knowsTarget(target.labelHandle) && jet != null && jet.canReach(target);
	}

	public override void execute() {
        base.execute();
		HoverJet jet = controller.GetComponentInChildren<HoverJet> ();
		if (jet != null) {
			jet.pursueTarget(target.labelHandle, false);
			jet.setAvailability(false);
		}
	}

	public override void stopExecution() {
        base.stopExecution();
		HoverJet jet = controller.GetComponentInChildren<HoverJet> ();
		if (jet != null) {
			jet.setTarget(null, false);
			jet.setAvailability(true);
		}
	}

	public override bool isStale() {
		return !controller.knowsTarget (target.labelHandle);
	}

	public override void onMessage(RobotMessage message) {
	}

	public override bool singleExecutor() {
		return false;
	}

	protected override float getCost() {
		HoverJet jet = controller.getRobotComponent<HoverJet> ();
		if (jet != null) {
			return jet.calculatePathCost(target);
		}
		return 0;
	}
}
