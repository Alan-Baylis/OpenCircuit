﻿using System.Collections.Generic;

[System.Serializable]
public class BuildTower : EndeavourFactory {

	private static List<TagRequirement> requiredTags = new List<TagRequirement> { new TagRequirement(TagEnum.BuildDirective, false) };

	protected override Endeavour createEndeavour(RobotController controller, Dictionary<TagEnum, Tag> tagMap) {
		return new BuildTowerAction(this, controller, goals, tagMap);
	}

	public static new List<TagRequirement> getRequiredTags() {
		return requiredTags;
	}
}
