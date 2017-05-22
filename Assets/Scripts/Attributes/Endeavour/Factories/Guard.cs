using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Guard : EndeavourFactory {

    private static List<TagRequirement> requiredTags = new List<TagRequirement> { new TagRequirement(TagEnum.GuardPoint) };

	protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tags) {
		return new GuardAction(this, controller, goals, tags);
	}

    public static new List<TagRequirement> getRequiredTags() {
        return requiredTags;
    }
}
