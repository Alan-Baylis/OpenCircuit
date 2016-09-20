using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
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
       return ((SentryDropPoint)factory).sentryModule != null;
    }

    public override void onMessage(RobotMessage message) {
        if (message.Type == RobotMessage.MessageType.ACTION && message.Message.Equals(HoverJet.TARGET_REACHED)) {
            dropSentry();
        }
	}

	public override System.Type[] getRequiredComponents() {
		return new System.Type[] { typeof(HoverJet) };
	}

	public override bool canExecute() {
        HoverJet legs = controller.getRobotComponent<HoverJet>();
        return legs != null && legs.canReach(parent.label);
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

    [Server]
    private void dropSentry() {
        ((SentryDropPoint)factory).getSentryModulePrefab().enabled = false;
        SentryModule newSentry = GameObject.Instantiate(((SentryDropPoint)factory).getSentryModulePrefab(), controller.transform.position - new Vector3(0, 1, 0), ((SentryDropPoint)factory).getSentryModulePrefab().transform.rotation) as SentryModule;
        ((SentryDropPoint)factory).sentryModule = newSentry.gameObject;
        newSentry.attachToController(controller);
        newSentry.enabled = true;
        NetworkServer.Spawn(newSentry.gameObject);
    }
}
