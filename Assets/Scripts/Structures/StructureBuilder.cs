using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class StructureBuilder : NetworkBehaviour {

	public float time = 10f;
	private Dictionary<Transform, Placement> positionMap = new Dictionary<Transform, Placement>();
	public List<Transform> buildOrder = new List<Transform>();

	public bool initialized;
	public float startTime;
	public float timePerObject;
	private int objectsSpawned;

	void Awake() {
		exploreTransform(transform);
		startTime = Time.time;
		timePerObject = time / buildOrder.Count;
		initialized = true;
	}

	[ClientCallback]
	void FixedUpdate () {
		if (initialized && startTime + timePerObject*objectsSpawned <= Time.time && buildOrder.Count > 0) {
			int chosen = Random.Range(0, buildOrder.Count);
			setEnabledIfPresent<MeshRenderer>(buildOrder[chosen], true);
			buildOrder.RemoveAt(chosen);
			++objectsSpawned;
		} else if (initialized && buildOrder.Count == 0) {
			Destroy(this);
		}
	}

	private void exploreTransform(Transform target) {
		positionMap[target] = new Placement(target.position, target.rotation);
		setEnabledIfPresent<MeshRenderer>(target, false);

		for (int i = 0; i < target.transform.childCount; ++i) {
			Transform child = target.transform.GetChild(i);
			exploreTransform(child);
		}
		buildOrder.Add(target);
	}

	private struct Placement {
		public readonly Vector3 position;
		public readonly Quaternion orientation;

		public Placement(Vector3 position, Quaternion orientation ) {
			this.position = position;
			this.orientation = orientation;
		}
	}

	private void setEnabledIfPresent<T>(Transform transform, bool enabled) where T : Component {
		T item = transform.GetComponent<T>();
		if (item != null) {
			Type itemType = item.GetType();
			if (itemType.GetProperty("enabled") != null) {
				itemType.GetProperty("enabled").SetValue(item, enabled, null);
			}
		}
	}
}
