using UnityEngine;
using System.Collections;

[System.Serializable]
public class DoorControlDestruction : Operation {

	[System.NonSerialized]
	private AutoDoor door;

	private static System.Type[] triggers = new System.Type[] {
		typeof(DestructTrigger),
	};

	public override System.Type[] getTriggers() {
		return triggers;
	}

	public override void perform(GameObject instigator, Trigger trig) {
		if(door != null) {
			door.removeDoorLock(parent);
		}
	}

	public void setDoor(AutoDoor door) {
		this.door = door;
	}
}
