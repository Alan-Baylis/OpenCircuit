using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SentryDropPoint : EndeavourFactory {

    private static List<TagRequirement> requiredTags = new List<TagRequirement> { new TagRequirement(TagEnum.SentryPoint, false) };

    protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tags) {
       return new DropSentryAction(this, controller, goals, tags);
    }
    public static new List<TagRequirement> getRequiredTags() {
        return requiredTags;
    }
}
