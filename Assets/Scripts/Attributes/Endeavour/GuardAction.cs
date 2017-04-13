using System.Collections.Generic;

public class GuardAction : Endeavour {

	Tag guardLocation;

	public GuardAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tagMap) : base(factory, controller, goals, tagMap){
		guardLocation = getTagOfType<Tag>(TagEnum.GuardPoint);
		name = "guard";
	}

	protected override void onExecute() {
		jet.setTarget(guardLocation.getLabelHandle(), true, true);
	}

	public override bool isStale() {
		return false;
	}

	public override System.Type[] getRequiredComponents() {
		return new [] { typeof(HoverJet) };
	}

	public override bool canExecute() {
		if(active && !jet.hasTarget() && !jet.hasReachedTarget(guardLocation.getLabelHandle())) {
			active = false;
		}
		return true;
	}

	public override bool singleExecutor() {
		return false;
	}

	protected override float getCost() {
		return jet.calculatePathCost(guardLocation.getLabelHandle().label);
	}

	public override TagEnum getPrimaryTagType() {
		return TagEnum.GuardPoint;
	}
}
