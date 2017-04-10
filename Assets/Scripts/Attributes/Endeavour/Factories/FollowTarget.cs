using System.Collections.Generic;
using UnityEditor;

[System.Serializable]
public class FollowTarget : EndeavourFactory {

	private static List<TagRequirement> requiredTags = new List<TagRequirement> { new TagRequirement(TagEnum.Team) };

	public float safetyMargin = 15f;

	protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tags) {
		return new FollowTargetAction(this, controller, goals, tags);
	}

	public new static List<TagRequirement> getRequiredTags() {
		return requiredTags;
	}

	public override void doGUI() {
		safetyMargin = EditorGUILayout.DelayedFloatField("Safety Margin", safetyMargin);
		EditorGUILayout.Separator();
		base.doGUI();
	}
}
