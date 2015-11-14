﻿using UnityEngine;
using System.Collections;

[System.Serializable]
public class Die : Operation {

	private static System.Type[] triggers = new System.Type[] {
		typeof(ElectricShock),
	};

	public override System.Type[] getTriggers() {
		return triggers;
	}

	public override void perform(GameObject instigator, Trigger trig) {
		Player player = parent.GetComponent<Player>();
		if (player != null) {
			player.die();
		}
	}

}
