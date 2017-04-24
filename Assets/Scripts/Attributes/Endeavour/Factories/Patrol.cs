using System.Collections.Generic;

[System.Serializable]
public class Patrol : EndeavourFactory {

    private static List<TagRequirement> requiredTags = new List<TagRequirement> { new TagRequirement(TagEnum.PatrolRoute) };

	protected override Endeavour createEndeavour (RobotController controller, Dictionary<TagEnum, Tag> tags) {
		return new PatrolAction(this, controller, goals, tags);
	}

    public new static List<TagRequirement> getRequiredTags() {
        return requiredTags;
    }
}
