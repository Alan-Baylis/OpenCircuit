using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PatrolAction : Endeavour {

	//private PatrolRoute route;
	//private List<Label> points;
	private List<LabelHandle> routePoints;
	private int currentDestination;

	public PatrolAction(EndeavourFactory factory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tagMap)
		: base(factory, controller, goals, tagMap) {
		//this.route = route;
		this.name = "patrol";
		//this.points = route;
		//routePoints = new List<Label> ();
		PatrolTag patrolTag = getTagOfType<PatrolTag>(TagEnum.PatrolRoute);
		if (patrolTag.getPoints() == null || patrolTag.getPoints().Count == 0) {
			if (patrolTag.getPoints().Count == 0) {
				Debug.LogWarning("Patrol route '" + patrolTag.getLabelHandle().label.name + "' has no route points");
			}
		}
		routePoints = patrolTag.getPointHandles();
		/*foreach (GameObject point in points) {
			Label label = point.GetComponent<Label>();
			if (label != null) {
				routePoints.Add(label);
			}
		}*/
	}

	protected override void onExecute() {
		HoverJet jet = controller.GetComponentInChildren<HoverJet> ();
		if (jet != null) {
			currentDestination = getNearest(controller.transform.position);
			jet.setTarget(routePoints[currentDestination], false);
		}
	}

	public override void onMessage(RobotMessage message) {
		if (message.Message.Equals (HoverJet.TARGET_REACHED)) {
			HoverJet jet = controller.GetComponentInChildren<HoverJet> ();
			if (jet != null && routePoints[currentDestination] == message.Target) {
				++currentDestination;
				if (currentDestination == routePoints.Count) {
					currentDestination = 0;
				}
				if(routePoints[currentDestination] == null) {
					Debug.LogWarning("Robot '" + controller.name + "' has detected a missing patrol route point. ");
					Debug.LogWarning("Robot '" + controller.name + "' halted. ");
				} else {
					jet.setTarget(routePoints[currentDestination], false);
				}
			}
		}
	}

	public override bool isStale() {
		return false;
	}

	public int getNearest(Vector3 position) {
		float minDist;
		int index = 0;
		minDist = Vector3.Distance(position, routePoints[0].label.transform.position);
		for (int i = 0; i < routePoints.Count; i++) {
			if(routePoints[i] == null) {
				Debug.LogWarning("Robot '"+controller.name+"' has detected a missing patrol route point!!!");
				continue;
			}
			float curDist = Vector3.Distance(position, routePoints[i].label.transform.position);
			if (curDist < minDist) {
				minDist = curDist;
				index = i;
			}
		}
		return index;
	}

	protected override float getCost() {
		HoverJet jet = controller.GetComponentInChildren<HoverJet> ();
		if (jet != null) {
			return jet.calculatePathCost(routePoints[currentDestination].label);
		}
		return 0;
	}

	public override System.Type[] getRequiredComponents() {
		return new System.Type[] { typeof(HoverJet) };
	}

	public override bool singleExecutor() {
		return false;
	}

	public override bool canExecute() {
		return true;
	}

	public override TagEnum getPrimaryTagType() {
		return TagEnum.PatrolRoute;
	}
}
