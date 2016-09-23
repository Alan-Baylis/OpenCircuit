using UnityEngine;

[System.Serializable]
public class OpenOperation : Operation {

	private static System.Type[] triggers = new System.Type[] {
		typeof(InteractTrigger),
	};

	[System.NonSerialized]
	public AutoDoor door;
	private string doorPath; 

	public override System.Type[] getTriggers() {
		return triggers;
	}

	public override void perform(GameObject instigator, Trigger trig) {
		if(getDoor() != null) {
			getDoor().toggle();
		}
    }

	public AutoDoor getDoor() {
		if (door == null) {
			door = ObjectReferenceManager.get().fetchReference<AutoDoor>(doorPath);
		}
		return door;
	}

#if UNITY_EDITOR
    public override void doGUI() {
		door = (AutoDoor)UnityEditor.EditorGUILayout.ObjectField(door, typeof(AutoDoor), true, null);
		ObjectReferenceManager.get().deleteReference(parent, doorPath);
		doorPath = ObjectReferenceManager.get().addReference(parent, door);
	}
#endif
}
