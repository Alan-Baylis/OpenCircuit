using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Guard : EndeavourFactory {

    private List<TagEnum> requiredTags = new List<TagEnum> { TagEnum.GuardPoint };

	protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tags) {
		return new GuardAction(this, controller, goals, tags);
	}

    public override List<TagEnum> getRequiredTags() {
        return requiredTags;
    }

    public override bool isApplicable(LabelHandle labelHandle) {
        return labelHandle.hasTag(TagEnum.GuardPoint);
    }
}
