using System.Collections.Generic;

public class GatherResourcesAction : Endeavour {

	private Tag target;

	public GatherResourcesAction(EndeavourFactory parentFactory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tagMap) : base(parentFactory, controller, goals, tagMap) {
		target = getTagOfType<Tag>(TagEnum.Resource);
		name = "gatherResources";
	}

	public override bool isStale() {
		return !controller.knowsTarget(target.getLabelHandle());
	}

	protected override void onExecute() {
		jet.setTarget(target.getLabelHandle(), true);
		arms.setTarget(target.getLabelHandle().label);

	}

	public override System.Type[] getRequiredComponents() {
		return new [] { typeof(HoverJet), typeof(AbstractArms)};
	}

	public override bool canExecute() {
		return jet.canReach(target.getLabelHandle().label);
	}

	public override bool singleExecutor() {
		return true;
	}

	public override TagEnum getPrimaryTagType() {
		return TagEnum.Resource;
	}

	protected override float getCost() {
		return jet.calculatePathCost(target.getLabelHandle().getPosition());
	}
}
