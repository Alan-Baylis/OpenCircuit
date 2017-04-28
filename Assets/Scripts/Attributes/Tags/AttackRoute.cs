using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
public class AttackRoute : AbstractRouteTag {

	private const float EXPIRATION_TIME = 5f;

	[System.NonSerialized]
	private int lastReachedNode;
	[System.NonSerialized]
	private List<NodeArrival> arrivals;
	[System.NonSerialized]
	private List<Squad> squads;
	[System.NonSerialized]
	public RouteState status = RouteState.UNKNOWN;

	[System.NonSerialized]
	private HashSet<RobotController> squadRobots;

	public AttackRoute(float severity, LabelHandle handle) : base(TagEnum.AttackRoute, severity, handle) {
	}

	public Vector3 getEndPoint() {
		return getPointHandles()[getPointHandles().Count - 1].getPosition();
	}

	public void updateLastReachedNode(int node) {
		if (node > lastReachedNode) {
			status = RouteState.PUSHING;
		}
		arrivals.Add(new NodeArrival(node, Time.time));
		for (int i = arrivals.Count - 1; i >= 0; --i) {
			if (Time.time - arrivals[i].timeStamp > EXPIRATION_TIME) {
				arrivals.RemoveAt(i);
			}
		}

		int prev = lastReachedNode;
		lastReachedNode = 0;
		foreach (NodeArrival arrival in arrivals) {
			if (arrival.node > lastReachedNode) {
				lastReachedNode = arrival.node;
			}
		}
		if (lastReachedNode < prev) {
			status = RouteState.IN_RETREAT;
		} else if (lastReachedNode == 0) {
			status = RouteState.UNKNOWN;
		}
	}

	public void setRallyNode(Squad squad) {
		status = RouteState.HOLDING;
		squad.setNode(getRallyNode());
	}

	public Squad formSquad(RobotController controller, int node) {
		foreach (Squad squad in squads) {
//			Debug.Log(squad.isReady() + " " + node + " " + squad.getNode());
			if (!squad.isReady() && squad.getNode() == node) {
				squadRobots.Add(controller);
				Debug.Log("add member");
				squad.addMember(controller);
				return squad;
			}
		}
		Debug.Log("Begin squad formation");
		Squad newSquad = new Squad(node);
		newSquad.addMember(controller);
		squads.Add(newSquad);
		squadRobots.Add(controller);
		return newSquad;
	}

	public int getLastReachedNode() {
		return lastReachedNode;
	}

	public int getRallyNode() {
		if (status == RouteState.PUSHING)
			return 0;
		return System.Math.Max(lastReachedNode - 2, 0);

	}

	public void removeFromSquad(RobotController controller, Squad squad) {
		squad.removeMember(controller);
		squadRobots.Remove(controller);
		if (squad.getMemberCount() == 0) {
			squads.Remove(squad);
		}
	}

	public enum RouteState {
		PUSHING, HOLDING, CONTESTED, IN_RETREAT, UNKNOWN
	}

	public bool isSquadRobot(RobotController controller) {
		return squadRobots.Contains(controller);
	}

	public bool isPreparingSquad() {
		foreach (Squad squad in squads) {
			if (!squad.isReady()) {
				return true;
			}
		}
		return false;
	}

	public class Squad {
		private List<RobotController> members = new List<RobotController>();
		private int size;
		private bool formed;
		private int node;


		public Squad(int node) {
			this.node = node;
			size = Random.Range(3, 4);
		}

		public void addMember(RobotController controller) {
			members.Add(controller);
			if (members.Count >= size) {
				formed = true;
				Debug.Log("Finish forming squad. Members: " + members.Count);
			}
		}

		public void removeMember(RobotController controller) {
			members.Remove(controller);
			if (members.Count < 2) {
				formed = false;
			}
		}

		public bool isReady() {
			return formed;
		}

		public int getMemberCount() {
			return members.Count;
		}

		public int getNode() {
			return node;
		}

		public void setNode(int node) {
			this.node = node;
		}
	}

	private struct NodeArrival {
		public readonly float timeStamp;
		public readonly int node;

		public NodeArrival(int node, float time) {
			timeStamp = time;
			this.node = node;
		}
	}

	[OnDeserialized]
	internal void onDeserialized(StreamingContext context) {
		arrivals = new List<NodeArrival>();
		squads = new List<Squad>();
		squadRobots = new HashSet<RobotController>();
	}

#if UNITY_EDITOR
	[System.NonSerialized]
	public float nodeDistance;
	public override void doGUI(GameObject parent) {
		base.doGUI(parent);
		UnityEditor.EditorGUILayout.LabelField("Average node distance", averagePointDistance(parent)+"");
		nodeDistance = UnityEditor.EditorGUILayout.FloatField("Node Distance", nodeDistance);
		if (GUILayout.Button("Apply to Path")) {

			for (int i = 0; i < getPoints(parent).Count-1; ++i) {
				fillInPoints(getPoints(parent)[i], getPoints(parent)[i + 1], parent, i);
			}
		}
	}

	private void fillInPoints(Label first, Label second, GameObject parent, int childIndex) {
		float remainingDistance = Vector3.Distance(first.transform.position, second.transform.position);
		Vector3 dir = second.transform.position - first.transform.position;
		dir.Normalize();
		Vector3 lastPos = first.transform.position;
		if (remainingDistance > nodeDistance+.5f) {
			GameObject newPoint = new GameObject();
			Label label = newPoint.AddComponent<Label>();
			label.isVisible = false;
			label.inherentKnowledge = false;

			newPoint.transform.position = dir * nodeDistance + lastPos;
			newPoint.transform.parent =  parent.transform;
			newPoint.transform.SetSiblingIndex(childIndex+1);
		}
	}

	private float averagePointDistance(GameObject parent) {
		List<Label> points = getPoints(parent);
		float distance = 0f;
		for (int i = 0; i < points.Count-1; ++i) {
			distance += Vector3.Distance(points[i].transform.position, points[i + 1].transform.position);

		}
		return distance / points.Count;
	}

	public override void drawGizmo(Label label) {
		//Color COLOR_ONE = Color.black;
		//Color COLOR_TWO = Color.green;
		Gizmos.color = Color.black;

		List<Label> points = getPoints(label.gameObject);
		for (int i = 0; i < points.Count-1; ++i) {
			if (getPoints(label.gameObject)[i] == null)
				continue;
			Color arrowColor = Color.white;
			float pointDistance = Vector3.Distance(points[i].transform.position, points[i + 1].transform.position);
			if (nodeDistance - pointDistance
			               < -.5f) {
				arrowColor = Color.blue;
			}
			if (nodeDistance - pointDistance > .5f) {
				arrowColor = Color.red;
			}
			int NUM_STRIPES = 8;
			Label current = getPoints(label.gameObject)[i];
			Label next = getPoints(label.gameObject)[i + 1];
			if (next == null || current == null) {
				return;
			}
			float LENGTH = Vector3.Distance(current.transform.position, next.transform.position);
			Vector3 dir = next.transform.position - current.transform.position;
			dir.Normalize();
			Quaternion rotation = Quaternion.LookRotation(dir);
			for (int j = 0; j < NUM_STRIPES * LENGTH; j = j + 2) {
				//Gizmos.color = j % 2 == 0 ? COLOR_ONE : COLOR_TWO;
				Vector3 startPos = current.transform.position + (j * dir / NUM_STRIPES);
				Vector3 endPos = startPos + dir / NUM_STRIPES;
				if (Vector3.Distance(current.transform.position, endPos) > LENGTH) {
					endPos = next.transform.position;
				}
				if (j % 8 == 0) {
					UnityEditor.Handles.color = arrowColor;
					UnityEditor.Handles.ConeCap(0, (startPos + endPos) / 2, rotation, .15f);
				} else {
					Gizmos.DrawLine(startPos, endPos);
				}
			}
		}
	}
#endif
}