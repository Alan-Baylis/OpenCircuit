﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

public class DropSentryAction : Endeavour {

	private Tag sentryPoint;

    public DropSentryAction(EndeavourFactory parentFactory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tags)
        : base(parentFactory, controller, goals, tags) {
        name = "dropSentry";
		sentryPoint = getTagOfType<Tag>(TagEnum.SentryPoint);
    }

	protected override void onExecute() {
		HoverJet jet = controller.getRobotComponent<HoverJet>();
        if (jet != null) {
            jet.setTarget(sentryPoint.getLabelHandle(), true);
        }
    }


    public override bool isStale() {
       return false;
    }

    public override void onMessage(RobotMessage message) {
        if (message.Type == RobotMessage.MessageType.ACTION && message.Message.Equals(HoverJet.TARGET_REACHED)) {
			getController().getRobotComponent<SentrySpawner>().dropSentry();
			sentryPoint.getLabelHandle().addTag(new Tag(TagEnum.Occupied, 0, sentryPoint.getLabelHandle()));
        }
	}

	public override System.Type[] getRequiredComponents() {
		return new System.Type[] { typeof(HoverJet), typeof(SentrySpawner) };
	}

	public override bool canExecute() {
        HoverJet legs = controller.getRobotComponent<HoverJet>();
        return legs != null && legs.canReach(sentryPoint.getLabelHandle().label) && !sentryPoint.getLabelHandle().hasTag(TagEnum.Occupied);
    }

    public override bool singleExecutor() {
        return true;
    }

    protected override float getCost() {
        HoverJet jet = controller.getRobotComponent<HoverJet>();
        if (jet != null) {
			return jet.calculatePathCost(sentryPoint.getLabelHandle().label.transform.position);
		}
		return 0;
    }

	public override TagEnum getPrimaryTagType() {
		return TagEnum.SentryPoint;
	}
}
