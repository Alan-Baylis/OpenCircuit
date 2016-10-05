using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Recharge : EndeavourFactory {

	public float rechargePoint = 1f;

    private List<TagEnum> requiredTags = new List<TagEnum>();

    public override Endeavour constructEndeavour(RobotController controller, LabelHandle handle, List<Tag> tags) {
		Battery battery = controller.GetComponentInChildren<Battery>();
		if (handle == null || battery == null) {
            return null;
        }
		RechargeAction action = new RechargeAction(this, controller, goals, handle.label, battery);
		action.rechargePoint = rechargePoint;
        return action;
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

#if UNITY_EDITOR
    public override void doGUI() {
		rechargePoint = UnityEditor.EditorGUILayout.FloatField("Recharge Point", rechargePoint);
		base.doGUI();
	}
#endif
}
