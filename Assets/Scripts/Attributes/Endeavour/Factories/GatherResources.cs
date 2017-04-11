using System.Collections.Generic;

[System.Serializable]
public class GatherResources : EndeavourFactory {

	private static List<TagRequirement> requiredTags = new List<TagRequirement> { new TagRequirement(TagEnum.Resource) };

	protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tagMap) {
		return new GatherResourcesAction(this, controller, goals, tagMap);
	}

	public new static List<TagRequirement> getRequiredTags() {
		return requiredTags;
	}
}
