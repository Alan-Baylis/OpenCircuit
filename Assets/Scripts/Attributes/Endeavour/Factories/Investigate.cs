using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Investigate : EndeavourFactory {

    private List<TagEnum> requiredTags = new List<TagEnum>();

    public override bool isApplicable(LabelHandle labelHandel) {
		return labelHandel.hasTag(TagEnum.Sound);
	}

	public override Endeavour constructEndeavour(RobotController controller, LabelHandle target, List<Tag> tags) {
		return new InvestigateAction(this, controller, this.goals, target);
	}

    public override List<TagEnum> getRequiredTags() {
        Debug.LogWarning(GetType().Name + " missing required tags!!!");
        return requiredTags;
    }
}
