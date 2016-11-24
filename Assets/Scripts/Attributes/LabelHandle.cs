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
        if (label != null) {
            return label.name;
        }
        return "";
    }

    public bool hasTag(TagEnum tagName) {
        if (label != null) {
            return label.hasTag(tagName);
        }
        return tagMap.ContainsKey(tagName);
    }

    public Tag getTag(TagEnum tagName) {
		if (label != null) {
			return label.getTag(tagName);
		}
		if (tagMap.ContainsKey(tagName)) {
			return tagMap[tagName];
		} else {
			return null;
		}
    }

    public List<TagEnum> getTagTypes() {
		if (label != null) {
			return label.getTagTypes();
		}
        List<TagEnum> tags = new List<TagEnum>();
        foreach (TagEnum tagEnum in tagMap.Keys) {
            tags.Add(tagEnum);
        }

        return tags;
    }

    public List<Tag> getTags() {
		if (label != null) {
			return label.getTags();
		} else {
			List<Tag> tags = new List<Tag>();
			tags.AddRange(tagMap.Values);
			return tags;
        }
    }

	public void addTag(Tag tag) {
		if (label != null) {
			label.setTag(tag);
		}
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
}
