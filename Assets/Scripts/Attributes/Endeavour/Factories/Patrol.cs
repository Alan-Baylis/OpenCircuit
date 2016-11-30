using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Patrol : EndeavourFactory {

    private List<TagEnum> requiredTags = new List<TagEnum> { TagEnum.PatrolRoute };

	protected override Endeavour createEndeavour (RobotController controller, Dictionary<TagEnum, Tag> tags) {
		return new PatrolAction(this, controller, goals, tags);
	}

    public override List<TagEnum> getRequiredTags() {
        return requiredTags;
    }
}
