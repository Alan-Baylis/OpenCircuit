using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Patrol : EndeavourFactory {

    private static List<TagEnum> requiredTags = new List<TagEnum> { TagEnum.PatrolRoute };

	protected override Endeavour createEndeavour (RobotController controller, Dictionary<TagEnum, Tag> tags) {
		return new PatrolAction(this, controller, goals, tags);
	}

    public static new List<TagEnum> getRequiredTags() {
        return requiredTags;
    }

    public override bool isApplicable(LabelHandle labelHandle) {
        return labelHandle.hasTag(TagEnum.PatrolRoute);
    }
}
