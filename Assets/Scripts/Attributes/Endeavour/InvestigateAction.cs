using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class InvestigateAction : Endeavour {

	private static float expirationTimeSeconds = 10; //Expires in 20 seconds

	//private System.DateTime creationTime;
	private float creationTime;
	private bool completed = false;
	private Tag sound;

	public InvestigateAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tags)
		: base(factory, controller, goals, tags) {
		//creationTime = System.DateTime.Now;
		creationTime = Time.time;
		this.name = "investigate";
		sound = getTagOfType<Tag>(TagEnum.Sound);
	}

	public override bool isStale() {
		NavMeshAgent nav = controller.GetComponent<NavMeshAgent>();

		return (active && nav.remainingDistance < 2f) || completed || (isComplete()) || !((Time.time - creationTime) < InvestigateAction.expirationTimeSeconds) || Vector3.Distance(controller.transform.position, sound.getLabelHandle().getPosition()) < 1.8f;
	}

	public bool isComplete() {

		RoboEyes eyes = controller.GetComponentInChildren<RoboEyes>();
		bool canSee = false;
		canSee = (eyes.lookAt(sound.getLabelHandle().getPosition()) == null);
		if(canSee) {
			controller.sightingLost(sound.getLabelHandle(), sound.getLabelHandle().getPosition(), null);
		}
		return canSee && Vector3.Distance(controller.transform.position, sound.getLabelHandle().getPosition()) < 5f;
	}

	public override void onMessage(RobotMessage message) {
		if(message.Type == RobotMessage.MessageType.ACTION) {
			if(message.Target == sound.getLabelHandle()) {
				completed = true;
				controller.sightingLost(sound.getLabelHandle(), sound.getLabelHandle().getPosition(), null);
			}
		}
	}

	public override System.Type[] getRequiredComponents() {
		return new System.Type[] { typeof(HoverJet) };
	}

	public override bool canExecute() {
		System.Nullable<Vector3> pos = controller.getLastKnownPosition(sound.getLabelHandle());
		return pos != null;
	}

	protected override void onExecute() {
		HoverJet jet = controller.GetComponentInChildren<HoverJet>();
		jet.setTarget(sound.getLabelHandle(), false);
	}

	public override bool singleExecutor() {
		return false;
	}

	protected override float getCost() {
		HoverJet jet = controller.GetComponentInChildren<HoverJet>();
		System.Nullable<Vector3> pos = controller.getLastKnownPosition(sound.getLabelHandle());
		if(pos.HasValue) {
			float cost = jet.calculatePathCost(pos.Value);
			//Debug.Log("investigate path cost: " + cost);
			return cost;
		}
		return 0;
	}

	public override TagEnum getPrimaryTagType() {
		return TagEnum.Sound;
	}
}
