using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LabelHandle {
	public Label label;
	public Dictionary<TagEnum, Tag> tagMap = new Dictionary<TagEnum, Tag>();

	protected Dictionary<System.Type, HashSet<RobotController>> executors = new Dictionary<System.Type, HashSet<RobotController>>();
	
	private Vector3 position;

	public LabelHandle(Vector3 pos, string name) {
		position = pos;
	}

	public string getName() {
		if(label != null) {
			return label.name;
		}
		return "";
	}

	public bool hasTag(TagEnum tagName) {
		if(label != null) {
			return label.hasTag(tagName);
		}
		return tagMap.ContainsKey(tagName);
	}

	public Tag getTag(TagEnum tagName) {
		return tagMap[tagName];
	}

	public void addTag(Tag tag) {
		tagMap.Add(tag.type, tag);
	}

	public Vector3 getPosition() {
		if(label != null) {
			return label.transform.position;
		}
		return position;
	}

	public void setPosition(Vector3 position) {
		if (label == null)
			this.position = position;
	}

	public Vector3? getDirection() {
		if(label != null) {
			Rigidbody rigidBody = label.GetComponent<Rigidbody>();
			if(rigidBody != null) {
				return rigidBody.velocity;
			}
		}
		return Vector3.zero;
	}

	public void addExecution(RobotController executor, System.Type endeavourType) {
		getExecutors(endeavourType).Add(executor);
	}

	public void removeExecution(RobotController executor, System.Type endeavourType) {
		getExecutors(endeavourType).Remove(executor);
	}

	public int getConcurrentExecutions(RobotController executor, System.Type endeavourType) {
		HashSet<RobotController> endeavourExecutors = getExecutors(endeavourType);
		if (endeavourExecutors.Contains(executor)) {
			return endeavourExecutors.Count - 1;
		}
		return endeavourExecutors.Count;
	}

	private HashSet<RobotController> getExecutors(System.Type endeavourType) {
		if (!getAllExecutors().ContainsKey(endeavourType)) {
			getAllExecutors()[endeavourType] = new HashSet<RobotController>();
		}
		return getAllExecutors()[endeavourType];
	}

	private Dictionary<System.Type, HashSet<RobotController>> getAllExecutors() {
		if (executors == null) {
			executors = new Dictionary<System.Type, HashSet<RobotController>>();
		}
		return executors;
	}
}
