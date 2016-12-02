using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Hunt : EndeavourFactory {

    private List<TagEnum> requiredTags = new List<TagEnum> { TagEnum.Player };

    protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tags) {
		return new HuntAction(this, controller, goals, tags);
	}

    public override List<TagEnum> getRequiredTags() {
        return requiredTags;
    }
}
