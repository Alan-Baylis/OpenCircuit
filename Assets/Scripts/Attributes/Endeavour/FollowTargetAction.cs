using System.Collections.Generic;
using UnityEngine;

public class FollowTargetAction : Endeavour {

	private Tag target;
	private FollowTarget parentFactory;

	public FollowTargetAction(EndeavourFactory parentFactory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tagMap) : base(parentFactory, controller, goals, tagMap) {
		target = getTagOfType<Tag>(TagEnum.Team);
		name = "followTarget";
		this.parentFactory = (FollowTarget) parentFactory;
	}

	public override void update() {
		float distance = Vector3.Distance(controller.transform.position, target.getLabelHandle().getPosition());
		if ( distance > parentFactory.safetyMargin || !canSeeTarget()) {
			jet.goToPosition(target.getLabelHandle().getPosition(), true);
		} else {
			jet.stop();
		}
	}

	public override bool isStale() {
		return !controller.knowsTarget(target.getLabelHandle())
		       || target.getLabelHandle().label == null
		       || target.getLabelHandle().label.GetComponent<Team>().team.Id == controller.GetComponent<Team>().team.Id;
	}

	protected override void onExecute() {
	}

	public override System.Type[] getRequiredComponents() {
		return new [] { typeof(HoverJet)};
	}

	public override bool canExecute() {
		return jet.canReach(target.getLabelHandle().getPosition()) && eyes != null;
	}

	public override bool singleExecutor() {
		return false;
	}

	public override TagEnum getPrimaryTagType() {
		return TagEnum.Team;
	}

	protected override float getCost() {
		float penalty = parentFactory.bonus;
		if (rifle != null && rifle.target == target.getLabelHandle()) {
			penalty = parentFactory.penalty;
		}
		return penalty + jet.calculatePathCost(target.getLabelHandle().getPosition());
	}

	private bool canSeeTarget() {
		GameObject found = eyes.lookAt(target.getLabelHandle().getPosition());
		return found == target.getLabelHandle().label.gameObject ||
		       found.transform.root.gameObject == target.getLabelHandle().label.gameObject;
	}
}
