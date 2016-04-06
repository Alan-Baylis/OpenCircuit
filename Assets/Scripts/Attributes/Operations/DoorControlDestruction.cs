using UnityEngine;
using System.Collections;

[System.Serializable]
public class DoorControlDestruction : Operation {

	[System.NonSerialized]
	public AutoDoor door;
	private string autoDoorRef = null;

	private static System.Type[] triggers = new System.Type[] {
		typeof(DestructTrigger),
	};

	public override System.Type[] getTriggers() {
		return triggers;
	}

	public override void perform(GameObject instigator, Trigger trig) {
		if(getDoor() != null) {
			getDoor().open();
			//TODO play a destruction sound here
			MonoBehaviour.Destroy(parent.gameObject);
		}
	}

	private AutoDoor getDoor() {
		if(door == null) {
			if (autoDoorRef == null) {
				door = null;
			} else {
				//door = ObjectReferenceManager.get()
				door = ObjectReferenceManager.get().fetchReference<AutoDoor>(autoDoorRef);
			}
		}
		return door;
	}

#if UNITY_EDITOR
	public override void doGUI() {
		door = (AutoDoor)UnityEditor.EditorGUILayout.ObjectField(getDoor(), typeof(AutoDoor), true);
		ObjectReferenceManager.get().deleteReference(autoDoorRef);
		autoDoorRef = ObjectReferenceManager.get().addReference(door);
		//damageType = UnityEditor.EditorGUILayout.TextField("Type", damageType);
		//damageAmount = UnityEditor.EditorGUILayout.FloatField("Amount", damageAmount); 
	}
#endif

}
