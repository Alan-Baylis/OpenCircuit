using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ScanAtStationFactory : EndeavourFactory {

    private List<TagEnum> requiredTags = new List<TagEnum>();

    public override Endeavour constructEndeavour(RobotController controller, LabelHandle handle, List<Tag> tags) {
		return new ScanAtStationAction(this, controller, goals, handle);
	}

    public override List<TagEnum> getRequiredTags() {
        Debug.LogWarning(GetType().Name + " missing required tags!!!");
        return requiredTags;
    }

    public override bool isApplicable(LabelHandle labelHandle) {
        //TODO: NO!!!
        Debug.LogWarning(GetType().Name + " isApplicable NYI!!!");
        return false;
    }
}
