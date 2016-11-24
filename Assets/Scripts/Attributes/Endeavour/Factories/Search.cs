using System.Collections.Generic;

[System.Serializable]
public class Search : EndeavourFactory {
    
    private static List<TagEnum> requiredTags = new List<TagEnum> { TagEnum.Searchable };

    protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tags) {
		return new SearchAction(this, controller, goals, tags);
	}

    public static new List<TagEnum> getRequiredTags() {
        return requiredTags; 
    }

    public override bool isApplicable(LabelHandle labelHandle) {
        return labelHandle.hasTag(TagEnum.Searchable);
    }
}
