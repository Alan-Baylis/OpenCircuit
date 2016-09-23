using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HuntAction : Endeavour {

	private Label target;

	public HuntAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Label target)
		: base(factory, controller, goals, target.labelHandle) {
		this.target = target;
		this.name = "hunt";
	}

	public override bool canExecute() {
		HoverJet jet = controller.getRobotComponent<HoverJet>();
		AbstractArms arms = controller.getRobotComponent<AbstractArms>();
        return arms != null
			&& (!target.hasTag(TagEnum.Grabbed) || arms.targetCaptured())
			&& jet != null && jet.canReach(target)
			&& target.GetComponent<Player>() != null && !target.GetComponent<Player>().frozen;
	}

	protected override void onExecute() {
		HoverJet jet = controller.getRobotComponent<HoverJet>();
		AbstractArms arms = controller.getRobotComponent<AbstractArms>();
		if (jet != null && arms != null && target != null) {
			jet.pursueTarget(target.labelHandle, false);
			arms.setTarget(target);
		}
	}

	public override bool isStale() {
		return target == null || !controller.knowsTarget(target.labelHandle);
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
			return jet.calculatePathCost(target);
		}
		return 0;
	}
}
