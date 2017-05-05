using UnityEngine;

[System.Serializable]
public class TutorialDamageOperation : Operation {

	private static System.Type[] triggers = {
		typeof(DamageTrigger)
	};

	public override void perform(GameObject instigator, Trigger trig) {
		Player player = instigator.GetComponentInParent<Player>();
		if (player != null) {
			parent.GetComponent<OnDamageTrigger>().doTheThing(player);
		}
	}

	public override System.Type[] getTriggers() {
		return triggers;
	}
}
