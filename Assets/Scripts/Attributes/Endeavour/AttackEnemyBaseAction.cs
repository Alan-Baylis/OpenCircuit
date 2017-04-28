using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AttackEnemyBaseAction : Endeavour {

	private List<LabelHandle> routePoints;
	private AttackRoute route;
	private int goalNode;
	private int currentDestination;
	private bool reached;

	private float ?pathLength;
	private AttackRoute.Squad squad;
	private Vector3? waitPoint;

	public AttackEnemyBaseAction(EndeavourFactory parentFactory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tagMap) : base(parentFactory, controller, goals, tagMap) {
		route = getTagOfType<AttackRoute>(TagEnum.AttackRoute);
		name = "attackBase";
		routePoints = route.getPointHandles();
	}

	public override void update() {
		if (squad == null) {
			if (reached) {
				squad = route.formSquad(getController(), goalNode);
				if (waitPoint == null && !squad.isReady()) {
					spreadOut();
				}
			} else {
				setGoalNode(route.getRallyNode());
			}
		} else if (squad.isReady()) {
			//Debug.Log("Squad ready!");
			waitPoint = null;
			setGoalNode(routePoints.Count - 1);
		} else {
			setGoalNode(route.getRallyNode());
			route.setRallyNode(squad);
			if (reached && waitPoint != null) {
				spreadOut();
			}
		}
	}

	private void spreadOut() {
		Vector3 randomPoint = 10 * Random.insideUnitSphere;
		randomPoint.y = 0f;
		NavMeshHit hit;
		while (!NavMesh.SamplePosition(randomPoint + routePoints[goalNode].getPosition(), out hit, 2f,
			NavMesh.AllAreas)) {
			waitPoint = hit.position;
			Vector3 dir = routePoints[goalNode + 1].getPosition() - routePoints[goalNode].getPosition();
			jet.goToPosition(hit.position, dir.normalized, true);
		}
	}

	public override bool isStale() {
		return route.getLabelHandle().label.GetComponent<TeamId>().id != controller.GetComponent<TeamId>().id;
	}

	protected override void onExecute() {
		currentDestination = getNearest(controller.transform.position);
		jet.setTarget(routePoints[currentDestination], false);
	}

	protected override void onStopExecution() {
		if (squad != null) {
			route.removeFromSquad(getController(), squad);
		}
	}

	public override System.Type[] getRequiredComponents() {
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
//			if (squad != null && squad.isReady())
			if (routePoints[currentDestination] == message.Target) {
				route.updateLastReachedNode(currentDestination);
				if (currentDestination == goalNode) {
					reached = true;
					return;
				}

				if (currentDestination < routePoints.Count - 1) {
					++currentDestination;
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
		return Vector3.Distance(controller.transform.position, routePoints[index].getPosition()) + getRemainingPathLength(goalNode);
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

	private void setGoalNode(int value) {
		if (value != goalNode) {
			reached = false;
			jet.setTarget(routePoints[value], false);
		}
		goalNode = value;
	}

//	protected override float calculateMobBenefit() {
//		int executors = tagMap[getPrimaryTagType()].getConcurrentExecutions(controller, GetType());
//		float mobEffect = 0;
//		if (route.isPreparingSquad()) {
//			mobEffect += factory.maxMobBenefit;
//		} else if (!route.isSquadRobot(getController())) {
//			mobEffect -= executors * factory.mobCostPerRobot;
//		}
//
//		return mobEffect;
//	}
}
