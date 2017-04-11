using System.Collections.Generic;
using UnityEditor;

[System.Serializable]
public class FollowTarget : EndeavourFactory {

	private static List<TagRequirement> requiredTags = new List<TagRequirement> { new TagRequirement(TagEnum.Team) };

	public float safetyMargin = 15f;
	public float bonus = 500f;
	public float penalty = -100f;

	protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tags) {
		return new FollowTargetAction(this, controller, goals, tags);
	}

	public new static List<TagRequirement> getRequiredTags() {
		return requiredTags;
	}

	public override void doGUI() {
		safetyMargin = EditorGUILayout.DelayedFloatField("Safety Margin", safetyMargin);
		bonus = EditorGUILayout.DelayedFloatField("Bonus", bonus);
		penalty = EditorGUILayout.DelayedFloatField("Penalty", penalty);
		EditorGUILayout.Separator();
		base.doGUI();
	}
}
