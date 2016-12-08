using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Recharge : EndeavourFactory {

	public float rechargePoint = 1f;

    private static List<TagRequirement> requiredTags = new List<TagRequirement> { new TagRequirement (TagEnum.PowerStation, false) };

    protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tags) {
		Battery battery = controller.GetComponentInChildren<Battery>();
		if (battery == null) {
            return null;
        }
		RechargeAction action = new RechargeAction(this, controller, goals, tags, battery);
		action.rechargePoint = rechargePoint;
        return action;
    }

    public static new List<TagRequirement> getRequiredTags() {
        return requiredTags;
    }

    public override bool isApplicable(LabelHandle labelHandle) {
        
        return labelHandle.hasTag(TagEnum.PowerStation);
    }

#if UNITY_EDITOR
    public override void doGUI() {
		rechargePoint = UnityEditor.EditorGUILayout.FloatField("Recharge Point", rechargePoint);
		base.doGUI();
	}
#endif
}
