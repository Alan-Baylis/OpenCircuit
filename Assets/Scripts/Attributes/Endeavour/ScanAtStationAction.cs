using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScanAtStationAction : Endeavour {

	LabelHandle scanStation;

	public ScanAtStationAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, LabelHandle scanStation)
		: base(factory, controller, goals, scanStation) {
		this.name = "scanAtStation";
		this.scanStation = scanStation;
	}

	public override System.Type[] getRequiredComponents() {
		return new System.Type[] { typeof(HoverJet) };
	}

	public override bool canExecute() {
		AbstractArms arms = controller.getRobotComponent<AbstractArms>();
		return (arms != null) && (arms.targetCaptured());
	}

	protected override void onExecute() {
		HoverJet jet = controller.GetComponentInChildren<HoverJet>();
		if(jet != null) {
			jet.setTarget(scanStation, true);
		}
	}


	public override bool isStale() {
		return scanStation == null;
	}

	public override void onMessage(RobotMessage message) {
		if(message.Type == RobotMessage.MessageType.ACTION) {
			if(message.Message.Equals(HoverJet.TARGET_REACHED)) {
				LaserProjector projector = scanStation.label.GetComponentInChildren<LaserProjector>();
				if(projector != null) {
					projector.setController(controller);
					projector.startScan();
				}
				//RobotArms arms = controller.GetComponentInChildren<RobotArms>();
				//arms.dropTarget();
			}
			else if(message.Message.Equals("target scanned")) {
				List<Goal> goals = new List<Goal>();
				goals.Add(new Goal(GoalEnum.Offense, 10f));
				AbstractArms arms = controller.GetComponentInChildren<AbstractArms>();
				Label target = arms.getTarget();
				if(target.GetComponent<Player>() != null) {
					Debug.LogWarning("Implement this using a stack instead!!");
					controller.addEndeavour(new ElectrocuteAction(factory, controller, goals, target));
				}
			}
		}
	}

	public override bool singleExecutor() {
		return true;
	}

	protected override float getCost() {
		HoverJet jet = controller.GetComponentInChildren<HoverJet>();
		if(jet != null) {
			return jet.calculatePathCost(scanStation.label);
		}
		return 0;
	}
}
