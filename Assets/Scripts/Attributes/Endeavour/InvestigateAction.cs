using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InvestigateAction : InherentEndeavour {

	private static float expirationTimeSeconds = 10; //Expires in 20 seconds

	//private System.DateTime creationTime;
	private float creationTime;
	private bool completed = false;

	public InvestigateAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, LabelHandle parent)
		: base(factory, controller, goals, parent) {
		//creationTime = System.DateTime.Now;
		creationTime = Time.time;
		this.name = "investigate";
		requiredComponents = new System.Type[] { typeof(HoverJet) };
	}



	public override bool isStale() {
		NavMeshAgent nav = controller.GetComponent<NavMeshAgent>();

		return (active && nav.remainingDistance < 2f) || completed || (isComplete()) || !((Time.time - creationTime) < InvestigateAction.expirationTimeSeconds) || Vector3.Distance(controller.transform.position, parent.getPosition()) < 1.8f;
	}

	public bool isComplete() {

		RoboEyes eyes = controller.GetComponentInChildren<RoboEyes>();
		bool canSee = false;
		if(eyes != null) {
			canSee = (eyes.lookAt(parent.getPosition()) == null);
			if(canSee) {
				controller.enqueueMessage(new RobotMessage(RobotMessage.MessageType.TARGET_LOST, "target lost", parent, parent.getPosition(), null));
			}
		}
		return canSee && Vector3.Distance(controller.transform.position, parent.getPosition()) < 5f;
	}

	public override void onMessage(RobotMessage message) {
		if(message.Type == RobotMessage.MessageType.ACTION) {
			if(message.Target == parent) {
				completed = true;
				controller.enqueueMessage(new RobotMessage(RobotMessage.MessageType.TARGET_LOST, "target lost", parent, parent.getPosition(), null));
			}
		}
	}

	public override bool canExecute() {
		System.Nullable<Vector3> pos = controller.getLastKnownPosition(parent);
		return pos != null;
	}

	public override void execute() {
		base.execute();
		HoverJet jet = controller.GetComponentInChildren<HoverJet>();
		if(jet != null) {
			jet.setTarget(parent, false);
			jet.setAvailability(false);
		}
	}

	public override void stopExecution() {
		base.stopExecution();
		HoverJet jet = controller.GetComponentInChildren<HoverJet>();
		if(jet != null) {
			jet.setTarget(null, false);
			jet.setAvailability(true);
		}
	}

	public override bool singleExecutor() {
		return false;
	}

	protected override float getCost() {
		HoverJet jet = controller.GetComponentInChildren<HoverJet>();
		if(jet != null) {
			System.Nullable<Vector3> pos = controller.getLastKnownPosition(parent);
			if(pos.HasValue) {
				float cost = jet.calculatePathCost(pos.Value);
				//Debug.Log("investigate path cost: " + cost);
				return cost;
			}
		}
		return 0;
	}
}
