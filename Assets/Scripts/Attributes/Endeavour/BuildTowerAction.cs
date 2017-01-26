using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildTowerAction : Endeavour {

	private Tag towerBase;
	private HoverJet jet;
	private TowerSpawner towerSpawner;

	public BuildTowerAction (EndeavourFactory factory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tags)
		: base(factory, controller, goals, tags) {
		towerBase = getTagOfType<Tag>(TagEnum.BuildDirective);
		this.name = "build tower";
		jet = getController().getRobotComponent<HoverJet>();
		towerSpawner = getController().getRobotComponent<TowerSpawner>();

	}

	public override bool canExecute() {
		return true;
	}

	public override TagEnum getPrimaryTagType() {
		return TagEnum.BuildDirective;
	}

	public override Type[] getRequiredComponents() {
		return new System.Type[] { typeof(HoverJet), typeof(TowerSpawner) };
	}

	public override bool isStale() {
		return towerBase.getLabelHandle().label == null;
	}

	public override bool singleExecutor() {
		return true;
	}

	public override void onMessage(RobotMessage message) {
		if (message.Message.Equals(HoverJet.TARGET_REACHED)) {
			towerSpawner.buildTower(towerBase.getLabelHandle().getPosition());
		}
	}

	protected override void onExecute() {
		jet.setTarget(towerBase.getLabelHandle(), true);
	}

	protected override float getCost() {
		return jet.calculatePathCost(towerBase.getLabelHandle().label);
	}
}
