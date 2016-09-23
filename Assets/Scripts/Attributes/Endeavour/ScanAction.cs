using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScanAction : Endeavour {
	private Label target;
	private bool isComplete = false;

	public ScanAction (EndeavourFactory factory, RobotController controller, List<Goal> goals, Label target) : base(factory, controller, goals, target.labelHandle) {
		this.target = target;
		this.name = "scan";
	}

	public override bool isStale() {
		return isComplete;
	}

	public override void onMessage(RobotMessage message) {
		if(message.Type == RobotMessage.MessageType.ACTION) {
			if(message.Message.Equals("target scanned")) {
				List<Goal> goals = new List<Goal>();
				goals.Add(new Goal(GoalEnum.Offense, 10f));
				controller.addEndeavour(new ElectrocuteAction(factory, controller, goals, target));
			}
		}
	}

	public override System.Type[] getRequiredComponents() {
		return new System.Type[] { typeof(HoverJet), typeof(AbstractArms), typeof(RoboEyes) };
	}

	public override bool canExecute() {
		AbstractArms arms = controller.getRobotComponent<AbstractArms>();
		RoboEyes eyes = controller.GetComponentInChildren<RoboEyes>();
		return eyes != null && eyes.hasScanner() && arms != null && arms.targetCaptured();
	}

	protected override void onExecute() {
		RoboEyes eyes = controller.GetComponentInChildren<RoboEyes>();
		if(eyes != null) {
			eyes.getScanner().startScan();
		}
	}

	protected override void onStopExecution() {
		RoboEyes eyes = controller.GetComponentInChildren<RoboEyes>();
		if(eyes != null) {
			eyes.getScanner().stopScan();
		}
	}

	public override bool singleExecutor() {
		return true;
	}

	protected override float getCost() {
		return 0;
	}
}
