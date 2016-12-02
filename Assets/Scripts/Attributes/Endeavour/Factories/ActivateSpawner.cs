using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class ActivateSpawner : EndeavourFactory {

    private List<TagEnum> requiredTags = new List<TagEnum> { TagEnum.Spawner };

    protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tags) {
		return new ActivateSpawnerAction(this, controller, goals, tags);
	}

    public override List<TagEnum> getRequiredTags() {
        return requiredTags;
    }
}
