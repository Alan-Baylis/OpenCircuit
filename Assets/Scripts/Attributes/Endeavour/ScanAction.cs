﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScanAction : Endeavour {
	private Label target;
	private bool isComplete = false;

	public ScanAction (RobotController controller, List<Goal> goals, Label target) : base(controller, goals, target.labelHandle) {
		this.target = target;
		this.name = "scan";
		requiredComponents = new System.Type[] {typeof(HoverJet)};
	}


	public override bool isStale() {
		return isComplete;
	}

	public override void onMessage(RobotMessage message) {
		if(message.Type == RobotMessage.MessageType.ACTION) {
			if(message.Message.Equals("target scanned")) {
				List<Goal> goals = new List<Goal>();
				goals.Add(new Goal(GoalEnum.Offense, 10f));
				controller.addEndeavour(new ElectrocuteAction(controller, goals, target));
			}
		}
	}

	public override bool canExecute() {
		RobotArms arms = controller.GetComponentInChildren<RobotArms>();
		RoboEyes eyes = controller.GetComponentInChildren<RoboEyes>();
		return eyes != null && eyes.hasScanner() && arms != null && arms.hasTarget();
	}

	public override void execute() {
		base.execute();
		RoboEyes eyes = controller.GetComponentInChildren<RoboEyes>();
		if(eyes != null) {
			eyes.getScanner().startScan();
		}
	}

	public override void stopExecution() {
		base.stopExecution();
		RoboEyes eyes = controller.GetComponentInChildren<RoboEyes>();
		if(eyes != null) {
			eyes.getScanner().stopScan();
		}
	}

	protected override float getCost() {
		return 0;
	}
}
