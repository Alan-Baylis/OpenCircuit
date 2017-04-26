using System.Collections.Generic;
using UnityEngine;

public abstract class BidBasedEndeavour : Endeavour {
	public BidBasedEndeavour(EndeavourFactory parentFactory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tagMap) : base(parentFactory, controller, goals, tagMap) {

	}

	public override bool isReady(Dictionary<System.Type, int> availableComponents) {
		return (!singleExecutor() || tagMap[getPrimaryTagType()].hasBid(getController(), GetType())) && canExecute() && hasAllComponents(availableComponents);
	}

	public override float getPriority() {
		if (lastFrameEvaluated != Time.frameCount) {
			priorityCache = calculateFinalPriority();
			lastFrameEvaluated = Time.frameCount;
			tagMap[getPrimaryTagType()].makeBid(getController(), GetType(), priorityCache);
		}
		return priorityCache;
	}

	protected virtual float calculateFinalPriority() {
		float finalPriority = calculatePriority();
		finalPriority -= getCost();
		return finalPriority;
	}

	public sealed override bool singleExecutor() {
		return true;
	}


}
