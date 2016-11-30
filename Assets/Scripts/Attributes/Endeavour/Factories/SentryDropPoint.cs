using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SentryDropPoint : EndeavourFactory {

    private List<TagEnum> requiredTags = new List<TagEnum> { TagEnum.SentryPoint };

    protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tags) {
       return new DropSentryAction(this, controller, goals, tags);
    }
    public override List<TagEnum> getRequiredTags() {
        return requiredTags;
    }
}
