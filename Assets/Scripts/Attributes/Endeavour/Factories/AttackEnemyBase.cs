using System.Collections.Generic;

[System.Serializable]
public class AttackEnemyBase : EndeavourFactory {

	private static List<TagRequirement> requiredTags = new List<TagRequirement> { new TagRequirement(TagEnum.AttackRoute) };

	protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tagMap) {
		return new AttackEnemyBaseAction(this, controller, goals, tagMap);
	}

	public new static List<TagRequirement> getRequiredTags() {
		return requiredTags;
	}
}
