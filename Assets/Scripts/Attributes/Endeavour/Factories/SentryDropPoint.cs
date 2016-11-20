using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SentryDropPoint : EndeavourFactory {

    [System.NonSerialized]
    public SentryModule sentryModule;

    private List<TagEnum> requiredTags = new List<TagEnum> { TagEnum.SentryPoint };

    protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tags) {
       return new DropSentryAction(this, controller, goals, tags);
    }
    public override List<TagEnum> getRequiredTags() {
        return requiredTags;
    }

    public override bool isApplicable(LabelHandle labelHandle) {
        return labelHandle.hasTag(TagEnum.SentryPoint);
    }
}
