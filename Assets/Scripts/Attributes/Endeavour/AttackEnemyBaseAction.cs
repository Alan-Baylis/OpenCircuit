using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackEnemyBaseAction : Endeavour {

	private List<LabelHandle> routePoints;
	private AttackRoute route;
	private int currentDestination;
	private bool reached;

	private float ?pathLength;

	public AttackEnemyBaseAction(EndeavourFactory parentFactory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tagMap) : base(parentFactory, controller, goals, tagMap) {
		route = getTagOfType<AttackRoute>(TagEnum.AttackRoute);
		name = "attackBase";
		routePoints = route.getPointHandles();
	}

	public override bool isStale() {
		return reached || route.getLabelHandle().label.GetComponent<TeamId>().id != controller.GetComponent<TeamId>().id;
	}

	protected override void onExecute() {
		currentDestination = getNearest(controller.transform.position);
		jet.setTarget(routePoints[currentDestination], false);	}

	public override Type[] getRequiredComponents() {
		return new [] { typeof(HoverJet) };
	}

	public override bool canExecute() {
		return true;
	}

	public override bool singleExecutor() {
		return false;
	}

	public override TagEnum getPrimaryTagType() {
		return TagEnum.AttackRoute;
	}

	public override void onMessage(RobotMessage message) {
		if (message.Message.Equals (HoverJet.TARGET_REACHED)) {
			if (routePoints[currentDestination] == message.Target) {
				++currentDestination;
				if (currentDestination == routePoints.Count) {
					reached = true;
					return;
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

	public int getNearest(Vector3 position) {
		float minDist;
		int index = 0;
		minDist = Vector3.Distance(position, routePoints[0].label.transform.position);
		for (int i = 0; i < routePoints.Count; ++i) {
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

		if (index < routePoints.Count - 1) {
			index = Vector3.Distance(position, routePoints[index + 1].label.transform.position)
			        < Vector3.Distance(routePoints[index].label.transform.position,
				        routePoints[index + 1].label.transform.position)
				? index + 1
				: index;
		}

		return index;
	}

	protected override float getCost() {
		int index = getNearest(controller.transform.position);
		return Vector3.Distance(controller.transform.position, routePoints[index].getPosition()) + getRemainingPathLength(index);
	}

	private float getRemainingPathLength(int nodeIndex) {
//		float pathLength = 0f;
//		for (int i = nodeIndex; i < routePoints.Count-1; ++i) {
//				if(routePoints[i] == null) {
//					Debug.LogWarning("Robot '"+controller.name+"' has detected a missing patrol route point!!!");
//					continue;
//				}
//				pathLength += Vector3.Distance(routePoints[i].label.transform.position, routePoints[i+1].label.transform.position);
//			}

		return getPathLength() * (1f - ((float)nodeIndex/(float)routePoints.Count));
	}

	private float getPathLength() {
		if (pathLength == null) {
			pathLength = 0f;
			for (int i = 0; i < routePoints.Count - 1; ++i) {
				if (routePoints[i] == null) {
					Debug.LogWarning("Robot '" + controller.name + "' has detected a missing patrol route point!!!");
					continue;
				}
				pathLength += Vector3.Distance(routePoints[i].label.transform.position,
					routePoints[i + 1].label.transform.position);
			}
		}
		return pathLength.Value;
	}
}
