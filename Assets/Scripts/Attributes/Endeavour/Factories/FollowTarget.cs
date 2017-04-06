using System.Collections.Generic;

[System.Serializable]
public class FollowTarget : EndeavourFactory {

	private static List<TagRequirement> requiredTags = new List<TagRequirement> { new TagRequirement(TagEnum.Team) };

	protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tags) {
		return new FollowTargetAction(this, controller, goals, tags);
	}

	public new static List<TagRequirement> getRequiredTags() {
		return requiredTags;
	}
}
