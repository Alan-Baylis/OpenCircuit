using System.Collections.Generic;

[System.Serializable]
public class InvestigateLostPlayer : EndeavourFactory {

	private static List<TagRequirement> requiredTags = new List<TagRequirement> { new TagRequirement(TagEnum.Player, true) };

	protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tagMap) {
		return new InvestigateLostPlayerAction(this, controller, goals, tagMap);
	}

	public static new List<TagRequirement> getRequiredTags() {
		return requiredTags;
	}
}
