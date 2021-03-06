﻿using System.Collections.Generic;
using UnityEngine;

public class InvestigateLostPlayerAction : Endeavour {

	private Tag player;
	private bool reached;

#if UNITY_EDITOR
	private GameObject mySphere;
	private GameObject sphere {
		get {
			if (mySphere == null) {
				mySphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				mySphere.GetComponent<MeshRenderer>().material.color = Color.cyan;
				GameObject.Destroy(mySphere.GetComponent<SphereCollider>());
			}
			return mySphere;
		}
	}
#endif
	public InvestigateLostPlayerAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tags) : base(factory, controller, goals, tags) {
		name = "investigateLostPlayer";
		player = getTagOfType<Tag>(TagEnum.Player);
	}

	public override void update() {
		jet.goToPosition(getController().getMentalModel().getLastKnownPosition(player.getLabelHandle()), false);
#if UNITY_EDITOR
		sphere.transform.position = getController().getMentalModel().getLastKnownPosition(player.getLabelHandle()).Value;
#endif
	}

	public override bool canExecute() {
		return true;
	}

	public override TagEnum getPrimaryTagType() {
		return TagEnum.Player;
	}

	public override System.Type[] getRequiredComponents() {
		return new [] { typeof(HoverJet) };
	}

	public override bool isStale() {
		if (player.getLabelHandle().label == null)
			return true;

		bool isAlly = false;
		if (GlobalConfig.globalConfig.gamemode is Bases) {
			isAlly = player.getLabelHandle().label.GetComponent<TeamId>().id == getController().GetComponent<TeamId>().id;
		}
		return reached || isAlly;
	}

	public override bool singleExecutor() {
		return false;
	}

	public override void onMessage(RobotMessage message) {
		if (message.Message == HoverJet.TARGET_REACHED) {
			reached = true;
		}
	}


	protected override float getCost() {
		return jet.calculatePathCost(getController().getMentalModel().getLastKnownPosition(player.getLabelHandle()).Value);
	}

	protected override void onExecute() {

	}

#if UNITY_EDITOR
	protected override void onStopExecution() {
		GameObject.Destroy(sphere);
	}
#endif

}
