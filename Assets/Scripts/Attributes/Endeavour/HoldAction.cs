using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HoldAction : Endeavour {

	Label target;

	bool hasComplained = false;

	public HoldAction(RobotController controller, Label target, LabelHandle source) : base(controller, new List<Goal>{new Goal(GoalEnum.Offense, 10), new Goal(GoalEnum.Protection, 3)}, source ) {
		this.target = target;
		this.name = "grab";
		requiredComponents = new System.Type[] {typeof(AbstractArms), typeof(HoverJet)};
	}

	public override bool canExecute () {
		return target != null;
	}

	public override void execute (){
        base.execute();
		AbstractArms arms = controller.GetComponentInChildren<AbstractArms>();
		if (arms != null) {
			arms.attachTarget(target);
			arms.setAvailability(false);
		}
	}

	public override void stopExecution(){
        base.stopExecution();
		AbstractArms arms = controller.GetComponentInChildren<AbstractArms>();
		if (arms != null) {
			arms.setAvailability(true);
			arms.dropTarget();
		}
	}

	public override bool isStale() {
		AbstractArms arms = controller.GetComponentInChildren<AbstractArms>();

        return target == null || arms == null || (arms.getProposedTarget() != target && !arms.hasTarget());
	}

	public void setTarget(Label target) {
		this.target = target;
	}

	public override void onMessage(RobotMessage message) {
	}

	protected override float getCost() {
		if(!hasComplained) { 
			Debug.LogWarning ("Please remind CryHavoc to implement the cost function for the HoldAction!!!");
			hasComplained = true;
		}
		return 0;
	}
}
