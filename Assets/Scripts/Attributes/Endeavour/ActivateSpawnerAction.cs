using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActivateSpawnerAction : Endeavour {

	RobotSpawner spawner;

	public ActivateSpawnerAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, LabelHandle parent, RobotSpawner spawner) : base(factory, controller, goals, parent) {
		this.spawner = spawner;
		name = "ActivateSpawner";
	}

	public override System.Type[] getRequiredComponents() {
		return new System.Type[] { typeof(HoverJet) };
    }

	public override bool canExecute() {
		return true;
	}

	protected override void onExecute() {
		getHoverJet().setTarget(parent, true);
	}

	public override bool isStale() {
		return spawner == null || spawner.active;
	}

	public override void onMessage(RobotMessage message) {
		if (message.Type == RobotMessage.MessageType.ACTION && message.Message == HoverJet.TARGET_REACHED) {
			spawner.active = true;
		}
    }

	public override bool singleExecutor() {
		return true;
	}

	protected override float getCost() {
		return getHoverJet().calculatePathCost(parent.label);
	}

	private HoverJet getHoverJet() {
		return controller.getRobotComponent<HoverJet>();
	}
}
