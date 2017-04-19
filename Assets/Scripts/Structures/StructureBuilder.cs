using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class StructureBuilder : NetworkBehaviour {

	public float time = 10f;
	public float margin = 1f;
	public List<Transform> buildOrder = new List<Transform>();

	private List<MoveJob> moveJobList = new List<MoveJob>();

	public bool initialized;
	public bool completed;
	public bool waiting;
	public float startTime;
	public float timePerObject;
	private int objectsSpawned;

	private StructureBuilder parent;
	private NetworkParenter parenter;

	private List<StructureBuilder> children = new List<StructureBuilder>();

	private float buildTime {
		get { return time - margin; }
	}

	public float timeRemaining {
		get { return startTime + buildTime - Time.time; }
	}

	void Awake() {
		parenter = transform.GetComponent<NetworkParenter>();

		exploreTransform(transform);
		if (parenter == null) {
			startTime = Time.time;
			timePerObject = buildTime / buildOrder.Count;

			initialized = true;
		}
	}

	[ClientCallback]
	void Update () {
		if (completed)
			return;
		if (waiting) {
			foreach (StructureBuilder builder in children) {
				if (!builder.completed) {
					return;
				}
			}
			if (parent != null) {
				completed = true;
			} else {
				initializeStructure(transform);
				cleanup();
			}
		}
		else if (initialized) {
			if (Time.time > startTime + time) {
				Destroy(this);
			}

			if (startTime + timePerObject * objectsSpawned <= Time.time && buildOrder.Count > 0) {
				int chosen = Random.Range(0, buildOrder.Count);

				GameObject copy = copyGameObject(buildOrder[chosen].gameObject);
				copy.transform.position = buildOrder[chosen].transform.position + Vector3.up;
				moveJobList.Add(new MoveJob(copy, buildOrder[chosen].gameObject, buildOrder[chosen].transform.position));
				buildOrder.RemoveAt(chosen);

				++objectsSpawned;
			} else if (buildOrder.Count == 0 && moveJobList.Count == 0) {
				waiting = true;
			}

			for (int i = moveJobList.Count - 1; i >= 0; --i) {
				MoveJob job = moveJobList[i];
				Vector3 startPos = job.copy.transform.position;
				if (Vector3.Distance(startPos, job.targetPos) < .001f) {
					moveJobList.RemoveAt(i);
					Destroy(job.copy);
					setEnabledIfPresent<MeshRenderer>(job.original.transform, true);

				} else {
					job.copy.transform.position = Vector3.MoveTowards(startPos, job.targetPos, Time.deltaTime
					                                                                           * Vector3.Distance(startPos,
						                                                                           job.targetPos)
					                                                                           /
					                                                                           (startTime + timePerObject *
					                                                                            (objectsSpawned) - Time.time));
				}
			}
		} else if (parenter != null && transform.parent != null){
			parent = transform.root.GetComponent<StructureBuilder>();
			if (parent != null) {
				if (!parent.completed) {
					parent.addChild(this);
					time = parent.timeRemaining;
					startTime = parent.startTime;
					timePerObject = buildTime / buildOrder.Count;
					initialized = true;
				}
			}
		}
	}

	public void addChild(StructureBuilder child) {
		children.Add(child);
	}

	private void cleanup() {
		foreach (StructureBuilder child in children) {
			child.cleanup();
		}
		Destroy(this);
	}

	private void initializeStructure(Transform target) {
		enableScripts(target);

		for (int i = 0; i < target.transform.childCount; ++i) {
			Transform child = target.transform.GetChild(i);
			initializeStructure(child);
		}
	}

	private void exploreTransform(Transform target) {
		setEnabledIfPresent<MeshRenderer>(target, false);
		disableScripts(target);

		for (int i = 0; i < target.transform.childCount; ++i) {
			Transform child = target.transform.GetChild(i);
			exploreTransform(child);
		}
		if (target.GetComponent<MeshRenderer>() != null) {
			buildOrder.Add(target);
		}
	}

	private void enableScripts(Transform target) {
		MonoBehaviour [] scripts = target.GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour script in scripts) {
			if (script != this) {
				script.enabled = true;
			}
		}
	}

	private void disableScripts(Transform target) {
		MonoBehaviour [] scripts = target.GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour script in scripts) {
			if (script != this) {
				script.enabled = false;
			}
		}
	}

	private GameObject copyGameObject(GameObject original) {
		GameObject copy = new GameObject();
		copy.transform.localScale = original.transform.localScale;
		copy.transform.rotation = original.transform.rotation;
		copyMeshFilter(original.GetComponent<MeshFilter>(), copy);
		copyMeshRenderer(original.GetComponent<MeshRenderer>(), copy);

		return copy;
	}

	private void copyMeshRenderer(MeshRenderer original, GameObject target) {
		if (original != null) {
			MeshRenderer copy = target.AddComponent<MeshRenderer>();
			copy.material = original.material;
		}

	}

	private void copyMeshFilter(MeshFilter original, GameObject target) {
		if (original != null) {
			MeshFilter copy = target.AddComponent<MeshFilter>();
			copy.mesh = original.mesh;
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

	private struct Placement {
		public readonly Vector3 position;
		public readonly Quaternion orientation;

		public Placement(Vector3 position, Quaternion orientation ) {
			this.position = position;
			this.orientation = orientation;
		}
	}

	private struct MoveJob {
		public readonly GameObject copy;
		public readonly GameObject original;
		public readonly Vector3 targetPos;

		public MoveJob(GameObject copy, GameObject original, Vector3 targetPos) {
			this.copy = copy;
			this.original = original;
			this.targetPos = targetPos;
		}
	}
}
