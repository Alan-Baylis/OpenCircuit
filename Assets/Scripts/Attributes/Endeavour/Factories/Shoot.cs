using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Shoot : EndeavourFactory {

    private static List<TagRequirement> requiredTags = new List<TagRequirement> { new TagRequirement(TagEnum.Team, false) };

    protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tagMap) {
        return new ShootAction(this, controller, goals, tagMap);
    }

    public static new List<TagRequirement> getRequiredTags() {
        return requiredTags;
    }
}
