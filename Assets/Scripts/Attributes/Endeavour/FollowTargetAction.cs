using System.Collections.Generic;
using UnityEngine;

public class FollowTargetAction : Endeavour {

	private Tag target;
	private float safetyMargin = 15f;

	public FollowTargetAction(EndeavourFactory parentFactory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tagMap) : base(parentFactory, controller, goals, tagMap) {
		target = getTagOfType<Tag>(TagEnum.Team);
		name = "followTarget";
	}

	public override void update() {
		jet.goToPosition(getTargetPos(), true);
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
		return jet.canReach(getTargetPos());
	}

	public override bool singleExecutor() {
		return false;
	}

	public override TagEnum getPrimaryTagType() {
		return TagEnum.Team;
	}

	protected override float getCost() {
		float penalty = 500f;
		if (rifle != null && rifle.target == target.getLabelHandle()) {
			penalty = -100f;
		}
		return penalty + jet.calculatePathCost(getTargetPos());
	}

	private Vector3 getTargetPos() {
		Vector3 adjust = controller.transform.position - target.getLabelHandle().getPosition();
		adjust.Normalize();
		return target.getLabelHandle().getPosition() + adjust * safetyMargin;
	}
}
