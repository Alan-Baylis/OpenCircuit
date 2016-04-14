using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlayerDeathOperation : Operation {

	private static System.Type[] triggers = new System.Type[] {
		typeof(DestructTrigger),
	};

	public override System.Type[] getTriggers() {
		return triggers;
	}

	public override void perform(GameObject instigator, Trigger trig) {
		Player player = parent.GetComponent<Player>();
		if(player != null) {
			player.die();
		}
	}
}
