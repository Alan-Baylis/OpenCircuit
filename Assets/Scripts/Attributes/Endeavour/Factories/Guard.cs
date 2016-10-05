using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Guard : EndeavourFactory {

    private List<TagEnum> requiredTags = new List<TagEnum> { TagEnum.GuardPoint };

	public override Endeavour constructEndeavour(RobotController controller, LabelHandle handle, List<Tag> tags) {
		return new GuardAction(this, controller, goals, handle.label);
	}

    public override List<TagEnum> getRequiredTags() {
        return requiredTags;
    }

    public override bool isApplicable(LabelHandle labelHandle) {
        return labelHandle.hasTag(TagEnum.GuardPoint);
    }
}
