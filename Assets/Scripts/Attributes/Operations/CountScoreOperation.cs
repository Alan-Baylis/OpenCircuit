﻿using UnityEngine;

[System.Serializable]
public class CountScoreOperation : Operation {

	private static System.Type[] triggers = {
		typeof(DestructTrigger)
	};

	public override System.Type[] getTriggers() {
		return triggers;
	}

	public override void perform(GameObject instigator, Trigger trig) {

		Player player = instigator.GetComponentInParent<Player>();
		Score score = parent.GetComponent<Score>();
		if (score != null && player != null) {
			score.recordScore(player.clientController);
		}
	}
}