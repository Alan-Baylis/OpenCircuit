using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Investigate : EndeavourFactory {

    private static List<TagRequirement> requiredTags = new List<TagRequirement> { new TagRequirement(TagEnum.Sound) };

	protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tags) {
		return new InvestigateAction(this, controller, this.goals, tags);
	}

    public static new List<TagRequirement> getRequiredTags() {
        return requiredTags;
    }
}
