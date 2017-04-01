using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Patrol : EndeavourFactory {

    private static List<TagRequirement> requiredTags = new List<TagRequirement> { new TagRequirement(TagEnum.PatrolRoute) };

	protected override Endeavour createEndeavour (RobotController controller, Dictionary<TagEnum, Tag> tags) {
		return new PatrolAction(this, controller, goals, tags);
	}

    public static new List<TagRequirement> getRequiredTags() {
        return requiredTags;
    }
}
