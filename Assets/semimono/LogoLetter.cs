using System.Collections.Generic;
using UnityEngine;

public class LogoLetter : MonoBehaviour {

	public bool loop;
	public float stickModifier = 1;

	private Dictionary<Transform, Transform> connections;
	private Dictionary<Transform, int> hits;

	public void Start () {
		hits = new Dictionary<Transform, int>();
		connections = new Dictionary<Transform, Transform>();
		Transform[] points = transform.GetComponentsInChildren<Transform>();
		Transform last = loop ? points[points.Length -1] : null;
		foreach (Transform child in points) {
			hits.Add(child, 0);
			if (last != null)
				connections.Add(last, child);
			last = child;
		}
	}

	public bool isNeighbor(Transform one, Transform two) {
		Transform value1, value2;
		connections.TryGetValue(one, out value1);
		connections.TryGetValue(two, out value2);
		return two == value1 || one == value2;
	}

	public void addHit(Transform point) {
		++hits[point];
	}

	public int getHits(Transform point) {
		return hits[point];
	}
}
