using UnityEngine;
using System.Collections.Generic;
using System;

public class SearchAction : Endeavour {
	
	public float agePriorityMultiplier = 0.05f;
	protected float lastSeen = -30;

	private Tag searchPoint;
	private HoverJet jet;

	public SearchAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tags) : base(factory, controller, goals, tags) {
		this.name = "search";
		searchPoint = getTagOfType<Tag>(TagEnum.Searchable);
		jet = getController().getRobotComponent<HoverJet>();
	}

	public override System.Type[] getRequiredComponents() {
		return new System.Type[] { typeof(HoverJet) };
	}

	public override bool canExecute() {
		return true;
	}

	public override bool isStale() {
		return false;
	}

	protected override void onExecute() {
		jet.setTarget(searchPoint.getLabelHandle(), true);
	}

	public override void onMessage(RobotMessage message) {
		if (message.Type == RobotMessage.MessageType.ACTION && message.Message == HoverJet.TARGET_REACHED) {
			lastSeen = Time.time;
		}
	}
	
    public override bool singleExecutor() {
        return false;
    }

    protected override float calculatePriority() {
        float priority = base.calculatePriority();
        priority *= (1 - Mathf.Min(1 / (Time.time - lastSeen) / agePriorityMultiplier, 1));
        return priority;
    }

	protected override float getCost() {
		return (jet == null) ? 0 : jet.calculatePathCost(searchPoint.getLabelHandle().getPosition()) * 0.5f;
	}

	public override TagEnum getPrimaryTagType() {
		return TagEnum.Searchable;
	}
}
