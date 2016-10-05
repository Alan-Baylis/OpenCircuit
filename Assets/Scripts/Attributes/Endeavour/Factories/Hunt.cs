using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Hunt : EndeavourFactory {

    private List<TagEnum> requiredTags = new List<TagEnum> { TagEnum.Player };

    public override Endeavour constructEndeavour(RobotController controller, LabelHandle handle, List<Tag> tags) {
		return new HuntAction(this, controller, goals, handle.label);
	}

    public override List<TagEnum> getRequiredTags() {
        return requiredTags;
    }

    public override bool isApplicable(LabelHandle labelHandle) {
        return labelHandle.hasTag(TagEnum.Player) && !labelHandle.hasTag(TagEnum.Grabbed);
    }
}
