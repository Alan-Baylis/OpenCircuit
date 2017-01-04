using System.Collections.Generic;
using UnityEngine;

public class InvestigateLostPlayerAction : Endeavour {

	private Tag player;

	public InvestigateLostPlayerAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tags) : base(factory, controller, goals, tags) {
		this.name = "investigateLostPlayer";
		this.player = getTagOfType<Tag>(TagEnum.Player);
	}

	public override void update() {
		HoverJet jet = getController().getRobotComponent<HoverJet>();
		jet.goToPosition(getController().getMentalModel().getLastKnownPosition(player.getLabelHandle()), false);
	}

	public override bool canExecute() {
		return true;
	}

	public override TagEnum getPrimaryTagType() {
		return TagEnum.Player;
	}

	public override System.Type[] getRequiredComponents() {
		return new System.Type[] { typeof(HoverJet) };
	}

	public override bool isStale() {
		return false;
	}

	public override bool singleExecutor() {
		return false;
	}

	protected override float getCost() {
		return getController().getRobotComponent<HoverJet>().calculatePathCost(getController().getMentalModel().getLastKnownPosition(player.getLabelHandle()).Value);
	}

	protected override void onExecute() {

	}
}
