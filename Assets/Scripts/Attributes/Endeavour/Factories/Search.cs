using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Search : EndeavourFactory {

    private List<TagEnum> requiredTags = new List<TagEnum> { TagEnum.Searchable };

    public override Endeavour constructEndeavour(RobotController controller, LabelHandle handle, List<Tag> tags) {
		return new SearchAction(this, controller, goals, handle);
	}

    public override List<TagEnum> getRequiredTags() {
        return requiredTags;
    }

    public override bool isApplicable(LabelHandle labelHandle) {
        return labelHandle.hasTag(TagEnum.Searchable);
    }
}
