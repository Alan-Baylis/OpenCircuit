using UnityEngine;
using System.Collections.Generic;

public class GuardAction : Endeavour {

	Label guardLocation;

	public GuardAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Label guardLocation) : base(factory, controller, goals, guardLocation.labelHandle){
		this.guardLocation = guardLocation;
		this.name = "guard";
	}

	protected override void onExecute() {
		HoverJet jet = controller.GetComponentInChildren<HoverJet>();
		if(jet != null ) {
			jet.setTarget(guardLocation.labelHandle, true, true);
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
		if(this.active && !jet.hasTarget() && !jet.hasReachedTarget(guardLocation.labelHandle)) {
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
			return jet.calculatePathCost(guardLocation);
		}
		return 0;
	}
}
