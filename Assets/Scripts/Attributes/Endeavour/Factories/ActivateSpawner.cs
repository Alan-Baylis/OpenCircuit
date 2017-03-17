using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class ActivateSpawner : EndeavourFactory {

    private static List<TagRequirement> requiredTags = new List<TagRequirement> { new TagRequirement(TagEnum.Spawner)};

    protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tags) {
		return new ActivateSpawnerAction(this, controller, goals, tags);
	}

    public static new List<TagRequirement> getRequiredTags() {
		return requiredTags;
    }
}
