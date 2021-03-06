﻿using UnityEngine;

[System.Serializable]
public class PlayerDeathOperation : Operation {

	private static System.Type[] triggers = {
		typeof(DestructTrigger)
	};

	public override System.Type[] getTriggers() {
		return triggers;
	}

	public override void perform(GameObject instigator, Trigger trig) {
		Player player = parent.GetComponent<Player>();
	    //TODO: frozen players are invincible. This may bite us one day...
		if(player != null && !player.frozen) {
			player.die();
		}
	}
}
