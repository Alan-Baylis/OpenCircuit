using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Hunt : EndeavourFactory {

    private static List<TagRequirement> requiredTags = new List<TagRequirement> { new TagRequirement(TagEnum.Player, false) };

    protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tags) {
		return new HuntAction(this, controller, goals, tags);
	}

    public static new List<TagRequirement> getRequiredTags() {
        return requiredTags;
    }

    public override bool isApplicable(LabelHandle labelHandle) {
        return labelHandle.hasTag(TagEnum.Player) && !labelHandle.hasTag(TagEnum.Grabbed);
    }
}
