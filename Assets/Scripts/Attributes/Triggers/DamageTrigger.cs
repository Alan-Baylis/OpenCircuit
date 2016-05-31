using UnityEngine;
using System.Collections;

public class DamageTrigger : Trigger {

	private float amount;

	public DamageTrigger(float amount) {
		this.amount = amount;
	}

	public float getAmount() {
		return amount;
	}
}
