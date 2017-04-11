using System.Collections.Generic;

public class ActivateSpawnerAction : Endeavour {

	private RobotSpawner spawner;

	public ActivateSpawnerAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tagMap) : base(factory, controller, goals, tagMap) {
		name = "ActivateSpawner";
		spawner = getTagOfType<Tag>(TagEnum.Spawner).getLabelHandle().label.GetComponentInChildren<RobotSpawner>();
	}

	public override System.Type[] getRequiredComponents() {
		return new [] { typeof(HoverJet) };
    }

	public override bool canExecute() {
		return true;
	}

	protected override void onExecute() {
		jet.setTarget(getTagOfType<Tag>(TagEnum.Spawner).getLabelHandle(), true);
	}

	public override bool isStale() {
		return spawner == null || spawner.active;
	}

	public override void onMessage(RobotMessage message) {
		if (message.Message == HoverJet.TARGET_REACHED) {
			spawner.active = true;
		}
    }

	public override bool singleExecutor() {
		return true;
	}

	protected override float getCost() {
		return jet.calculatePathCost(getTagOfType<Tag>(TagEnum.Spawner).getLabelHandle().label);
	}

	public override TagEnum getPrimaryTagType() {
		return TagEnum.Spawner;
	}
}
