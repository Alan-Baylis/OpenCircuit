using UnityEngine;
using System.Collections.Generic;

public class SearchAction : InherentEndeavour {
	
	public float agePriorityMultiplier = 0.05f;
	protected float lastSeen = -30;

	public SearchAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, LabelHandle parent) : base(factory, controller, goals, parent) {
		this.name = "search";
		requiredComponents = new System.Type[] { typeof(HoverJet) };
	}

	public override bool canExecute() {
		return true;
	}

	public override bool isStale() {
		return false;
	}

	public override void execute() {
		base.execute();
		controller.getRobotComponent<HoverJet>().setTarget(parent, true);
	}

	public override void onMessage(RobotMessage message) {
		if (message.Type == RobotMessage.MessageType.ACTION && message.Message == "target reached") {
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
        HoverJet jet = controller.getRobotComponent<HoverJet>();
		return (jet == null) ? 0 : jet.calculatePathCost(parent.getPosition()) * 0.5f;
	}
}
