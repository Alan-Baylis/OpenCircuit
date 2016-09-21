using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ObjectReferenceManager : MonoBehaviour {

	[SerializeField]
	private ReferenceDictionary references;
	
	[System.NonSerialized]
	private static ObjectReferenceManager instance;

	public ObjectReferenceManager() {
		if (instance == null) {
			instance = this;
		}
		if (references == null)
			references = new ReferenceDictionary();
	}

	public void Start() {
		lock(typeof(ObjectReferenceManager)) {
			if (instance != null && instance != this) {
				GameObject.DestroyImmediate(gameObject);
				Debug.LogError("Cleaned up ref manager!  Go yell at semimono: this should never happen, and it's his fault!");
			} else {
				instance = this;
			}
		}
		gameObject.hideFlags = HideFlags.HideInHierarchy;
	}

	public static ObjectReferenceManager get() {
		lock(typeof(ObjectReferenceManager)) {
			if (instance == null)
				instance = new GameObject("ObjectReferenceManager").AddComponent<ObjectReferenceManager>();
		}
		return instance;
	}

	public string addReference(Object you, Object obReference) {
		if (obReference == null)
			return null;
		string id = System.Guid.NewGuid().ToString();
		references[id] = new Reference(you, obReference);
		return id;
	}

	public void updateReference(string id, Object obReference) {
		if (references.ContainsKey(id))
			references[id] = new Reference(references[id], obReference);
		else
			Debug.LogError("Cannot update object reference with non-existant ID: " +id);
	}

	public bool deleteReference(Object you, string refId) {
		if (refId == null) return false;
        if (references.ContainsKey(refId) && references[refId].isOwner(you)) {
            references.Remove(refId);
            return true;
        } else {
            return false;
        }
	}

	public T fetchReference<T>(string refId) where T: Object {
		if (refId == null)
			return null;
		Reference ob;
		if (!references.TryGetValue(refId, out ob)) {
			Debug.LogError("Broken object reference");
		}
		return (T) ob.obj;
	}

	[System.Serializable]
	protected class ReferenceDictionary: SerializableDictionary<string, Reference> {}

    [System.Serializable]
    protected struct Reference {
        private UnityEngine.Object owner;
        public UnityEngine.Object obj;

        public Reference(UnityEngine.Object owner, UnityEngine.Object obj) {
            this.owner = owner;
            this.obj = obj;
        }

        public Reference(Reference original, UnityEngine.Object obj) {
            this.owner = original.owner;
            this.obj = obj;
        }

        public bool isOwner(UnityEngine.Object owner) {
            return this.owner == owner;
        }
    }
}
