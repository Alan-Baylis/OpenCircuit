using System.Collections.Generic;

[System.Serializable]
public class FollowTarget : EndeavourFactory {

	private static List<TagRequirement> requiredTags = new List<TagRequirement> { new TagRequirement(TagEnum.Team) };

	public float safetyMargin = 15f;
	public float bonus = 500f;
	public float penalty = -100f;

	protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tagMap) {
		return new FollowTargetAction(this, controller, goals, tagMap);
	}

	public new static List<TagRequirement> getRequiredTags() {
		return requiredTags; 
	}

#if UNITY_EDITOR
	public override void doGUI() {
		safetyMargin = UnityEditor.EditorGUILayout.DelayedFloatField("Safety Margin", safetyMargin);
		bonus = UnityEditor.EditorGUILayout.DelayedFloatField("Bonus", bonus);
		penalty = UnityEditor.EditorGUILayout.DelayedFloatField("Penalty", penalty);
		UnityEditor.EditorGUILayout.Separator();
		base.doGUI();
	}
#endif
}
