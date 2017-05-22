using UnityEngine;
using System.Collections.Generic;

public class InvestigateAction : Endeavour {

	private static float expirationTimeSeconds = 10;

	private readonly float creationTime;
	private readonly SoundTag sound;
	private bool sighted;

	public InvestigateAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tags)
		: base(factory, controller, goals, tags) {
		creationTime = Time.time;
		name = "investigate";
		sound = getTagOfType<SoundTag>(TagEnum.Sound);
	}

	public override void update() {
		var canSee = eyes.lookAt(sound.getLabelHandle().getPosition()) == null;
		if (canSee && controller.knowsTarget(sound.getLabelHandle())) {
			controller.sightingLost(sound.getLabelHandle(), sound.getLabelHandle().getPosition(), null);
		}
		sighted = canSee && Vector3.Distance(controller.transform.position, sound.getLabelHandle().getPosition()) < 5f;
	}

	public override bool isStale() {
		UnityEngine.AI.NavMeshAgent nav = controller.GetComponent<UnityEngine.AI.NavMeshAgent>();

		return active && nav.remainingDistance < 2f
			|| sighted
			|| !(Time.time - creationTime < expirationTimeSeconds)
			|| Vector3.Distance(controller.transform.position, sound.getLabelHandle().getPosition()) < 1.8f
		    || !controller.knowsTarget(sound.getLabelHandle());
	}

	public override System.Type[] getRequiredComponents() {
		return new [] { typeof(HoverJet) };
	}

	public override bool canExecute() {
		Vector3? pos = controller.getLastKnownPosition(sound.getLabelHandle());
		return pos != null;
	}

	protected override void onExecute() {
		jet.setTarget(sound.getLabelHandle(), false);
	}

	public override bool singleExecutor() {
		return false;
	}

	protected override float getCost() {
		Vector3? pos = controller.getLastKnownPosition(sound.getLabelHandle());
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
