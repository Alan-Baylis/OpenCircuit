using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class HuntAction : Endeavour {

	private Tag target;

	public HuntAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tags)
		: base(factory, controller, goals, tags) {
		target = getTagOfType<Tag>(TagEnum.Player);
		this.name = "hunt";
	}

	public override bool canExecute() {
		HoverJet jet = controller.getRobotComponent<HoverJet>();
		AbstractArms arms = controller.getRobotComponent<AbstractArms>();
		return (!target.getLabelHandle().hasTag(TagEnum.Grabbed) || arms.targetCaptured())
			&& !target.getLabelHandle().hasTag(TagEnum.Frozen)
			&& jet.canReach(target.getLabelHandle().label);
	}

	protected override void onExecute() {
		HoverJet jet = controller.getRobotComponent<HoverJet>();
		AbstractArms arms = controller.getRobotComponent<AbstractArms>();
		jet.setTarget(target.getLabelHandle(), false);
		arms.setTarget(target.getLabelHandle().label);
	}

	public override bool isStale() {
		return !controller.knowsTarget(target.getLabelHandle());
	}

	public override void onMessage(RobotMessage message) {
		if(message.Type == RobotMessage.MessageType.ACTION && message.Message.Equals(AbstractArms.TARGET_CAPTURED_MESSAGE)) {
            HoverJet jet = controller.getRobotComponent<HoverJet>();
            jet.stop();
		} else if (message.Type == RobotMessage.MessageType.ACTION && message.Message.Equals(AbstractArms.RELEASED_CAPTURED_MESSAGE)) {
			HoverJet jet = controller.getRobotComponent<HoverJet>();
			jet.pursueTarget(target.getLabelHandle(), false);
		}
	}

	public override System.Type[] getRequiredComponents() {
		return new System.Type[] { typeof(HoverJet), typeof(AbstractArms) };
	}

	public override bool singleExecutor() {
		return false;
	}

	protected override float getCost() {
		HoverJet jet = controller.getRobotComponent<HoverJet>();
		return jet.calculatePathCost(target.getLabelHandle().label);
	}

	public override TagEnum getPrimaryTagType() {
		return TagEnum.Player;
	}
}
