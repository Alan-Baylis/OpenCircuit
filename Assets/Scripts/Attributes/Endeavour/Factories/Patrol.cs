using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Patrol : EndeavourFactory {

    private List<TagEnum> requiredTags = new List<TagEnum> { TagEnum.PatrolRoute };

	public override Endeavour constructEndeavour (RobotController controller, LabelHandle handle, List<Tag> tags) {
        PatrolTag patrolTag = ((PatrolTag)tags[0]);
        if (handle == null || patrolTag.getPoints() == null || patrolTag.getPoints().Count == 0) {
			if(patrolTag.getPoints().Count == 0) {
				Debug.LogWarning("Patrol route '"+handle.label.name+"' has no route points");
			}
			return null;
		}
		return new PatrolAction(this, controller, goals, patrolTag.getPointHandles(), handle.label);
	}

    public override List<TagEnum> getRequiredTags() {
        return requiredTags;
    }

    public override bool isApplicable(LabelHandle labelHandle) {
        return labelHandle.hasTag(TagEnum.PatrolRoute);
    }
}
