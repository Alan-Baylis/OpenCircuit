using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildTowerAction : BidBasedEndeavour {

	private BuildDirectiveTag towerBase;

	public BuildTowerAction (EndeavourFactory factory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tags)
		: base(factory, controller, goals, tags) {
		towerBase = getTagOfType<BuildDirectiveTag>(TagEnum.BuildDirective);
		name = "build tower";
	}

	public override bool canExecute() {
		return true;
	}

	public override TagEnum getPrimaryTagType() {
		return TagEnum.BuildDirective;
	}

	public override Type[] getRequiredComponents() {
		return new [] { typeof(HoverJet), typeof(TowerSpawner) };
	}

	public override bool isStale() {
		return towerBase.getLabelHandle().label == null;
	}

	public override void onMessage(RobotMessage message) {
		if (message.Message.Equals(HoverJet.TARGET_REACHED)) {
			MonoBehaviour.Destroy(towerBase.getLabelHandle().label.gameObject);
			towerSpawner.buildTower(towerBase.getLabelHandle().getPosition(), towerBase);
			controller.getMentalModel().removeSighting(towerBase.getLabelHandle(), towerBase.getLabelHandle().getPosition(), null);
		}
	}

	protected override void onExecute() {
		jet.setTarget(towerBase.getLabelHandle(), true);
	}

	protected override float getCost() {
		return jet.calculatePathCost(towerBase.getLabelHandle().label);
	}
}
