using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class StructureBuilder : NetworkBehaviour {

	public float time = 10f;
	public float margin = 1f;
	public float vOffset;
	public MonoBehaviour[] leaveEnabled;
	public AbstractEffectController partBuiltEffect;

	private List<Transform> buildOrder = new List<Transform>();
	private List<MoveJob> moveJobList = new List<MoveJob>();

	private bool initialized;
	private bool completed;
	private bool waiting;
	private float startTime;
	private float timePerObject;
	private int objectsSpawned;
	private int partCount;

	private StructureBuilder parent;
	private NetworkParenter parenter;

	private List<StructureBuilder> children = new List<StructureBuilder>();

	private float buildTime {
		get { return time - margin; }
	}

	void Awake() {
		parenter = transform.GetComponent<NetworkParenter>();

		exploreTransform(transform);
		partCount = buildOrder.Count;
		if (parenter == null) {
			startTime = Time.time;
			timePerObject = buildTime / buildOrder.Count;

			initialized = true;
		}
	}

	[ClientCallback]
	void Update () {
		if (completed) {
			Destroy(this);
		} else if (waiting) {
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
			double percentComplete = (Time.time - startTime) /buildTime;

			while(buildOrder.Count > 0 && percentComplete *partCount > objectsSpawned) {
				int chosen = 0;//Random.Range(0, buildOrder.Count);

				GameObject copy = copyGameObject(buildOrder[chosen].gameObject);
				copy.transform.position = buildOrder[chosen].transform.position + Vector3.up *vOffset;
				moveJobList.Add(new MoveJob(copy, buildOrder[chosen].gameObject));
				buildOrder.RemoveAt(chosen);

				++objectsSpawned;
			}
			if (buildOrder.Count == 0 && moveJobList.Count == 0) {
				waiting = true;
			}

			for (int i = moveJobList.Count - 1; i >= 0; --i) {
				MoveJob job = moveJobList[i];
				float partPercentComplete = (Time.time - job.startTime) /timePerObject;
				if (partPercentComplete >= 1) {
					moveJobList.RemoveAt(i);
					GlobalConfig.globalConfig.effectsManager.spawnEffect(partBuiltEffect, job.copy.transform.position, Quaternion.identity);
					Destroy(job.copy);
					setEnabledIfPresent<MeshRenderer>(job.original.transform, true);
				} else {
					job.copy.transform.position = Vector3.Lerp(job.startPos, job.targetPos, partPercentComplete);
				}
			}
		} else if (parenter != null && transform.parent != null){
			parent = transform.root.GetComponent<StructureBuilder>();
			if (parent != null && !parent.completed) {
				parent.addChild(this);
				time = parent.time;
				margin = parent.margin;
				startTime = parent.startTime;
				timePerObject = buildTime / buildOrder.Count;
				initialized = true;
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
			if (script != this && !System.Array.Exists(leaveEnabled, e => e == script)) {
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
		public readonly Vector3 startPos;
		public readonly float startTime;

		public MoveJob(GameObject copy, GameObject original) {
			this.copy = copy;
			this.original = original;
			this.targetPos = original.transform.position;
			startPos = copy.transform.position;
			startTime = Time.time;
		}
	}
}
