using UnityEngine;
using System.Collections;

[System.Serializable]
public class MineDestruction : Operation {

	private static System.Type[] triggers = new System.Type[] {
		typeof(DestructTrigger),
	};

	public override System.Type[] getTriggers() {
		return triggers;
	}

	public override void perform(GameObject instigator, Trigger trig) {
		LandMine mine = parent.GetComponent<LandMine>();
		if (mine != null) {
			mine.detonate();
		}
	}
}
