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
        return arms != null
			&& (!target.getLabelHandle().hasTag(TagEnum.Grabbed) || arms.targetCaptured())
			&& jet != null && jet.canReach(target.getLabelHandle().label)
			&& target.getLabelHandle().label.GetComponent<Player>() != null && !target.getLabelHandle().label.GetComponent<Player>().frozen;
	}

	protected override void onExecute() {
		HoverJet jet = controller.getRobotComponent<HoverJet>();
		AbstractArms arms = controller.getRobotComponent<AbstractArms>();
		if (jet != null && arms != null && target != null) {
			jet.pursueTarget(target.getLabelHandle(), false);
			arms.setTarget(target.getLabelHandle().label);
		}
	}

	public override bool isStale() {
		return target == null || !controller.knowsTarget(target.getLabelHandle());
	}

	public override void onMessage(RobotMessage message) {
		if(message.Type == RobotMessage.MessageType.ACTION && message.Message.Equals(AbstractArms.TARGET_CAPTURED_MESSAGE)) {
            HoverJet jet = controller.getRobotComponent<HoverJet>();
            if (jet != null) {
                jet.stop();
            }
		}
	}

	public override System.Type[] getRequiredComponents() {
		return new System.Type[] { typeof(HoverJet), typeof(AbstractArms) };
	}

	public override bool singleExecutor() {
		return false;
	}

	protected override float getCost() {
        if (target == null) {
            return float.PositiveInfinity;
        }
		HoverJet jet = controller.getRobotComponent<HoverJet>();
		if(jet != null) {
			return jet.calculatePathCost(target.getLabelHandle().label);
		}
		return 0;
	}

	public override TagEnum getPrimaryTagType() {
		return TagEnum.Player;
	}
}
