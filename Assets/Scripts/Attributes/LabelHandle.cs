using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LabelHandle {
	public Label label;
	public Dictionary<TagEnum, Tag> tagMap = new Dictionary<TagEnum, Tag>();
	
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

	public Vector3? getDirection() {
		if(label != null) {
			Rigidbody rigidBody = label.GetComponent<Rigidbody>();
			if(rigidBody != null) {
				return rigidBody.velocity;
			}
		}
		return Vector3.zero;
	}
}
