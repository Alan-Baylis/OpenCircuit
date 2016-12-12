using System.Collections.Generic;

public class InvestigateLostPlayerAction : Endeavour {

	private Tag player;

	public InvestigateLostPlayerAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tags) : base(factory, controller, goals, tags) {
		this.name = "investigateListPlayer";
		this.player = getTagOfType<Tag>(TagEnum.Player);
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
		HoverJet jet = getController().getRobotComponent<HoverJet>();
				
	}
}
