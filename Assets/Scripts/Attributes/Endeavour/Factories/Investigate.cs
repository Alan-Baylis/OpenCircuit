using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Investigate : EndeavourFactory {

    private List<TagEnum> requiredTags = new List<TagEnum> { TagEnum.Sound };

    public override bool isApplicable(LabelHandle labelHandel) {
		return labelHandel.hasTag(TagEnum.Sound);
	}

	protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tags) {
		return new InvestigateAction(this, controller, this.goals, tags);
	}

    public override List<TagEnum> getRequiredTags() {
        return requiredTags;
    }
}
