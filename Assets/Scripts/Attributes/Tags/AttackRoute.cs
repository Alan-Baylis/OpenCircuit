using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackRoute : AbstractRouteTag {

	public AttackRoute(float severity, LabelHandle handle) : base(TagEnum.AttackRoute, severity, handle) {
	}

	public Vector3 getEndPoint() {
		return getPointHandles()[getPointHandles().Count - 1].getPosition();
	}

#if UNITY_EDITOR
	public float nodeDistance;
	public override void doGUI(GameObject parent) {
		base.doGUI(parent);
		UnityEditor.EditorGUILayout.LabelField("Average node distance", averagePointDistance(parent)+"", new GUILayoutOption[0]);
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