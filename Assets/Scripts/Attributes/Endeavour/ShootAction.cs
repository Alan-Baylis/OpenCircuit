﻿﻿using System;
using System.Collections.Generic;

public class ShootAction : Endeavour {

    private Tag target;

    public ShootAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tags)
        : base(factory, controller, goals, tags) {
        target = getTagOfType<Tag>(TagEnum.Team);
        name = "shoot";
    }

    public override bool isStale() {
        return !getController().knowsTarget(target.getLabelHandle())
               || target.getLabelHandle().label == null
               || target.getLabelHandle().label.GetComponent<TeamId>().id == controller.GetComponent<TeamId>().id
               || !target.getLabelHandle().hasTag(TagEnum.Health);
    }

    protected override void onExecute() {
        rifle.setTarget(target.getLabelHandle());
    }

    public override Type[] getRequiredComponents() {
        return new[] {typeof(AbstractRobotGun) };
    }

    public override bool canExecute() {
        return !target.getLabelHandle().hasTag(TagEnum.Frozen) && !rifle.targetObstructed(target.getLabelHandle());
    }

    public override bool singleExecutor() {
        return false;
    }

    public override TagEnum getPrimaryTagType() {
        return TagEnum.Team;
    }

    protected override float getCost() {
        return 0f;
    }
}
