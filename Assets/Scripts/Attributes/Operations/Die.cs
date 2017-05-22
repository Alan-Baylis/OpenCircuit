using UnityEngine;

[System.Serializable]
public class Die : Operation {

	private static System.Type[] triggers = {
		typeof(ElectricShock)
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
