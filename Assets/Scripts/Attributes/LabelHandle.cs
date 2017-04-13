using UnityEngine;
using System.Collections.Generic;

public class LabelHandle {
	public Dictionary<TagEnum, Tag> tagMap = new Dictionary<TagEnum, Tag>();

	private bool backed;

	public Label label {
		get { return myLabel; }
		set {
			myLabel = value;
			backed = true;
		}
	}

	public bool isBacked {
		get { return backed; }
	}

	private Vector3 position;

	private Label myLabel;

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

	public bool hasTags() {
		if (label != null) {
			return label.hasTags();
		}
		return tagMap != null && tagMap.Count > 0;
	}

    public Tag getTag(TagEnum tagName) {
		if (label != null) {
			return label.getTag(tagName);
		}
		Tag value = null;
		tagMap.TryGetValue(tagName, out value);
		return value;
    }

    public List<TagEnum> getTagTypes() {
		if (label != null) {
			return label.getTagTypes();
		}
        List<TagEnum> tags = new List<TagEnum>(tagMap.Keys);
        return tags;
    }

    public List<Tag> getTags() {
		if (label != null) {
			return label.getTags();
		} else {
			List<Tag> tags = new List<Tag>(tagMap.Values);
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
