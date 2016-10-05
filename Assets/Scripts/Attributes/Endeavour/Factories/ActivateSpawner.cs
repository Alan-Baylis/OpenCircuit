using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class ActivateSpawner : EndeavourFactory {

    private List<TagEnum> requiredTags = new List<TagEnum> { TagEnum.Spawner };

    public override Endeavour constructEndeavour(RobotController controller, LabelHandle handle, List<Tag> tags) {
        //TODO is this the right way to get the robotSpawner??? (Hint: I think not...)
		return new ActivateSpawnerAction(this, controller, goals, handle, handle.label.GetComponent<RobotSpawner>());
	}

    public override List<TagEnum> getRequiredTags() {
        return requiredTags;
    }

    public override bool isApplicable(LabelHandle labelHandle) {
        return labelHandle.hasTag(TagEnum.Spawner) && labelHandle.hasTag(TagEnum.Inactive);
    }
}
