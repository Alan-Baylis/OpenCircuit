using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class DropSentryAction : Endeavour {

    public DropSentryAction(EndeavourFactory parentFactory, RobotController controller, List<Goal> goals, LabelHandle parent)
        : base(parentFactory, controller, goals, parent) {
        name = "dropSentry";
    }

	protected override void onExecute() {
		HoverJet jet = controller.getRobotComponent<HoverJet>();
        if (jet != null) {
            jet.setTarget(parent, true);
        }
    }


    public override bool isStale() {
       return false;
    }

    public override void onMessage(RobotMessage message) {
        if (message.Type == RobotMessage.MessageType.ACTION && message.Message.Equals(HoverJet.TARGET_REACHED)) {
            ((SentryDropPoint)factory).sentryModule = getController().getRobotComponent<SentrySpawner>().dropSentry();
        }
	}

	public override System.Type[] getRequiredComponents() {
		return new System.Type[] { typeof(HoverJet), typeof(SentrySpawner) };
	}

	public override bool canExecute() {
        HoverJet legs = controller.getRobotComponent<HoverJet>();
        return legs != null && legs.canReach(parent.label) && ((SentryDropPoint)factory).sentryModule == null;
    }

    public override bool singleExecutor() {
        return true;
    }

    protected override float getCost() {
        HoverJet jet = controller.getRobotComponent<HoverJet>();
        if (jet != null) {
            return jet.calculatePathCost(parent.label);
        }
        return 0;
    }
}
