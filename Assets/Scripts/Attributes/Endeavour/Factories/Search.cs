using System.Collections.Generic;

[System.Serializable]
public class Search : EndeavourFactory {
    
    private static List<TagRequirement> requiredTags = new List<TagRequirement> { new TagRequirement(TagEnum.Searchable, false) };

    protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tags) {
		return new SearchAction(this, controller, goals, tags);
	}

    public static new List<TagRequirement> getRequiredTags() {
        return requiredTags; 
    }
}
