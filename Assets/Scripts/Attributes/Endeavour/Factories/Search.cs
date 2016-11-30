using System.Collections.Generic;

[System.Serializable]
public class Search : EndeavourFactory {
    
    private List<TagEnum> requiredTags = new List<TagEnum> { TagEnum.Searchable };

    protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tags) {
		return new SearchAction(this, controller, goals, tags);
	}

    public override List<TagEnum> getRequiredTags() {
        return requiredTags; 
    }
}
