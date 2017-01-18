using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class HuntAction : Endeavour {

	private Tag target;
	private HoverJet jet;
	private AbstractArms arms;

	public HuntAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tags)
		: base(factory, controller, goals, tags) {
		target = getTagOfType<Tag>(TagEnum.Player);
		this.name = "hunt";
		jet = getController().getRobotComponent<HoverJet>();
		arms = getController().getRobotComponent<AbstractArms>();
	}

	public override bool canExecute() {
		return (!target.getLabelHandle().hasTag(TagEnum.Grabbed) || arms.targetCaptured())
			&& !target.getLabelHandle().hasTag(TagEnum.Frozen)
			&& jet.canReach(target.getLabelHandle().label);
	}

	protected override void onExecute() {
		jet.setTarget(target.getLabelHandle(), false);
		arms.setTarget(target.getLabelHandle().label);
	}

	public override bool isStale() {
		return !controller.knowsTarget(target.getLabelHandle());
	}

	public override void onMessage(RobotMessage message) {
		if(message.Type == RobotMessage.MessageType.ACTION && message.Message.Equals(AbstractArms.TARGET_CAPTURED_MESSAGE)) {
            jet.stop();
		} else if (message.Type == RobotMessage.MessageType.ACTION && message.Message.Equals(AbstractArms.RELEASED_CAPTURED_MESSAGE)) {
			jet.setTarget(target.getLabelHandle(), false);
		}
	}

	public override System.Type[] getRequiredComponents() {
		return new System.Type[] { typeof(HoverJet), typeof(AbstractArms) };
	}

	public override bool singleExecutor() {
		return false;
	}

	protected override float getCost() {
		return jet.calculatePathCost(target.getLabelHandle().label);
	}

	public override TagEnum getPrimaryTagType() {
		return TagEnum.Player;
	}
}
