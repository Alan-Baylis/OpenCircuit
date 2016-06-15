using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActivateSpawnerAction : Endeavour {

	RobotSpawner spawner;

	public ActivateSpawnerAction(RobotController controller, List<Goal> goals, LabelHandle parent, RobotSpawner spawner) : base(controller, goals, parent) {
		this.spawner = spawner;
		requiredComponents = new System.Type[] { typeof(HoverJet) };
		name = "ActivateSpawner";
	}

	private HoverJet getHoverJet() {
		return controller.getRobotComponent<HoverJet>();
	}

	public override bool canExecute() {
		return true;
	}

	public override void execute() {
		base.execute();
		getHoverJet().setTarget(parent, true);
	}

	public override bool isStale() {
		return spawner == null || spawner.active;
	}

	public override void onMessage(RobotMessage message) {
		if (message.Type == RobotMessage.MessageType.ACTION && message.Message == "target reached") {
			spawner.active = true;
		}
    }

	protected override float getCost() {
		return getHoverJet().calculatePathCost(parent.label);
	}
}
