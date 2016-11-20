using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[AddComponentMenu("Scripts/Labels/Label")]
public class Label : MonoBehaviour, ISerializationCallbackReceiver {

	[System.NonSerialized]
	public LabelHandle labelHandle;
	
	[System.NonSerialized]
	public static readonly HashSet<Label> labels = new HashSet<Label>();

	[System.NonSerialized]
	public static readonly HashSet<Label> visibleLabels = new HashSet<Label>();

	[SerializeField]
	public byte[] serializedData;

	[System.NonSerialized]
	public Operation[] operations = new Operation[0];

	[System.NonSerialized]
	private Dictionary<System.Type, List<Operation>> triggers = new Dictionary<System.Type, List<Operation>>();

	//Properties handled by Unity serialization
	public bool isVisible = true;
    public bool inherentKnowledge = false;

	[System.NonSerialized]
	public Tag[] tags = new Tag[0];

	[System.NonSerialized]
	public Dictionary<TagEnum, Tag> tagMap = new Dictionary<TagEnum, Tag>();

#if UNITY_EDITOR
    void Update() {
        if (tagMap.Count > tags.Length) {
            tags = new Tag[tagMap.Count];
            tagMap.Values.CopyTo(tags, 0);
        }
    }
#endif

    public void Awake() {
		labelHandle = new LabelHandle(transform.position, name);
		labelHandle.label = this;

		Label.labels.Add(this);
		if (isVisible) {
			visibleLabels.Add(this);
		}
		triggers = new Dictionary<System.Type, List<Operation>>();
		foreach(Operation op in operations) {
			addOperation(op, op.getTriggers());
		}

		tagMap = new Dictionary<TagEnum, Tag>();
		foreach(Tag tag in tags) {
			if (tag == null) {
				Debug.LogWarning("Null tag attached to '" + name + "'");
			}
			tag.setLabelHandle(labelHandle);
			tagMap.Add(tag.type, tag);
		}
	}

	public bool hasOperationType(System.Type type) {
		return triggers.ContainsKey(type);
	}

	public bool sendTrigger(GameObject instigator, Trigger trig) {
        System.Type type = trig.GetType();
		if (triggers.ContainsKey(type)) {
			foreach (Operation e in (List<Operation>)triggers[type]) {
                e.perform(instigator, trig);
            }
			return true;
		}
		return false;
	}

	public void addOperation(Operation operation, System.Type[] triggerTypes) {
        if (operation != null) {
			operation.setParent(this);
            foreach (System.Type element in triggerTypes) {
				if (!triggers.ContainsKey(element)) {
					triggers[element] = new List<Operation>();
                }
				((List<Operation>)triggers[element]).Add(operation);
            }
		}
	}

	/*public virtual List<Endeavour> getAvailableEndeavours (RobotController controller) {
		List<Endeavour> availableEndeavours = new List<Endeavour> ();
		foreach (EndeavourFactory endeavour in endeavours) {
			Endeavour newEndeavour = endeavour.constructEndeavour(controller);
			if(newEndeavour != null) {
				availableEndeavours.Add(newEndeavour);
			}
		}
		return availableEndeavours;
	}*/

	public void OnDestroy() {
		Label.labels.Remove(this);
	}

	public void OnBeforeSerialize() {
		lock(this) {
			MemoryStream stream = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();

			formatter.Serialize(stream, tags);
			formatter.Serialize(stream, operations);

			serializedData = stream.ToArray();
			stream.Close();
		}
	}
	
	public void OnAfterDeserialize() {
		lock(this) {
			MemoryStream stream = new MemoryStream(serializedData);
			BinaryFormatter formatter = new BinaryFormatter();

			tags = (Tag[])formatter.Deserialize(stream);
			operations = (Operation[]) formatter.Deserialize(stream);

			stream.Close();
		}
	}

	public void setTag(Tag tag) {
		tagMap.Add(tag.type, tag);
	}

	public void clearTag(TagEnum type) {
		tagMap.Remove(type);
	}

	public bool hasTag(TagEnum tagName) {
		return tagMap.ContainsKey(tagName);
	}

	public Tag getTag(TagEnum tagName) {
		return tagMap[tagName];
	}

	public List<TagEnum> getTagTypes() {
		List<TagEnum> tags = new List<TagEnum>();
		foreach (TagEnum tagEnum in tagMap.Keys) {
			tags.Add(tagEnum);
		}

		return tags;
	}

	public GameObject getGameObject() {
		return gameObject;
	}

#if UNITY_EDITOR
	void OnDrawGizmos() {
		if(!isVisible) {
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(transform.position, .2f);
		}
        foreach (Tag tag in tags) {
            if (tag == null)
                continue;
            tag.drawGizmo();
        }
	}
#endif
}