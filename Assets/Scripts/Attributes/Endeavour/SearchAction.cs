using UnityEngine;
using System.Collections.Generic;

public class SearchAction : InherentEndeavour {
	
	public float agePriorityMultiplier = 0.05f;
	protected float lastSeen = 0;

	public SearchAction(RobotController controller, List<Goal> goals, LabelHandle parent) : base(controller, goals, parent) {
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
		MonoBehaviour.print("message");
		if (message.Type == RobotMessage.MessageType.ACTION && message.Message == "target reached") {
			MonoBehaviour.print("reset time");
			lastSeen = Time.time;
		}
	}

	public override float getPriority() {
		float priority = base.getPriority();
		priority *= (1 -Mathf.Min(1 /(Time.time -lastSeen) /agePriorityMultiplier, 1));
		HoverJet jet = controller.getRobotComponent<HoverJet>();

		// this is a kind of hackish system to circumvent the order of applying priority modifiers
		float cost = (jet == null) ? 0 : jet.calculatePathCost(parent.getPosition()) *0.5f;
		return priority - cost;
	}

	protected override float getCost() {
		return 0;
	}
}
