using UnityEngine;
using System.Collections;

[System.Serializable]
public class DestructionOperation : Operation {

	private static System.Type[] triggers = {
		typeof(DestructTrigger)
	};

	public override System.Type[] getTriggers() {
		return triggers;
	}

	public override void perform(GameObject instigator, Trigger trig) {
		MonoBehaviour.Destroy(parent.gameObject);
	}

}
