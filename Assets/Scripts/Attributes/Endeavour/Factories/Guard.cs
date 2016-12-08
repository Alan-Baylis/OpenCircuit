﻿using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Guard : EndeavourFactory {

    private static List<TagRequirement> requiredTags = new List<TagRequirement> { new TagRequirement(TagEnum.GuardPoint, false) };

	protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tags) {
		return new GuardAction(this, controller, goals, tags);
	}

    public static new List<TagRequirement> getRequiredTags() {
        return requiredTags;
    }

    public override bool isApplicable(LabelHandle labelHandle) {
        return labelHandle.hasTag(TagEnum.GuardPoint);
    }
}
