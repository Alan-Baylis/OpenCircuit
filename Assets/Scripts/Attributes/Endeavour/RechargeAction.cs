using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class RechargeAction : Endeavour {

    private Tag powerStation;
	private Battery battery;
	public float rechargePoint = 1;

    public RechargeAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tagMap, Battery battery) : base(factory, controller, goals, tagMap) {
        powerStation = getTagOfType<Tag>(TagEnum.PowerStation);
        this.name = "recharge";
		this.battery = battery;
    }

	protected override void onExecute() {
		HoverJet jet = controller.GetComponentInChildren<HoverJet>();
        if (jet != null) {
            jet.setTarget(powerStation.getLabelHandle(), true);
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
			return jet.calculatePathCost(powerStation.getLabelHandle().label);
		}
        return 0f;
    }

	public override TagEnum getPrimaryTagType() {
		return TagEnum.PowerStation;
	}
}
