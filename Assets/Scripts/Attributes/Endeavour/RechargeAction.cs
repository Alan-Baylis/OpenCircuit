using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class RechargeAction : Endeavour {

    private Label powerStation;
	private Battery battery;
	public float rechargePoint = 1;

    public RechargeAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Label target, Battery battery) : base(factory, controller, goals, target.labelHandle) {
        powerStation = target;
        this.name = "recharge";
		this.battery = battery;
    }

	protected override void onExecute() {
		HoverJet jet = controller.GetComponentInChildren<HoverJet>();
        if (jet != null) {
            jet.setTarget(powerStation.labelHandle, true);
        }
    }

    public override bool isStale() {
        return powerStation == null;
	}

	public override System.Type[] getRequiredComponents() {
		return new System.Type[] { typeof(HoverJet) };
	}

	public override bool canExecute() {
		return ((battery.currentCapacity / battery.maximumCapacity) <= rechargePoint || active);
	}

	public override bool singleExecutor() {
		return false;
	}

    protected override float calculatePriority() {
        float batteryDrained = 2 - ((battery.currentCapacity / battery.maximumCapacity) * 2f);
        return base.calculatePriority() * batteryDrained;
    }

    protected override float getCost() {
		HoverJet jet = controller.GetComponentInChildren<HoverJet>();
		if(jet != null) {
			return jet.calculatePathCost(powerStation);
		}
        return 0f;
    }
}
