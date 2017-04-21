using UnityEngine;

[System.Serializable]
public class DamageOperation : Operation {
	
	private static System.Type[] triggers = new System.Type[] {
		typeof(DamageTrigger)
	};

	[System.NonSerialized]
	private Health health;

	public DamageOperation(Health healthScript) {
		health = healthScript;
	}

	//public string damageType;
	//public float damageAmount;
	
	public override System.Type[] getTriggers() {
		return triggers;
	}
	
	public override void perform(GameObject instigator, Trigger trig) {
		DamageTrigger trigger = (DamageTrigger)trig;
		if(health != null) {
			health.hurt(trigger.getAmount(), instigator);
		}

	}

#if UNITY_EDITOR
    public override void doGUI() {
		//damageType = UnityEditor.EditorGUILayout.TextField("Type", damageType);
		//damageAmount = UnityEditor.EditorGUILayout.FloatField("Amount", damageAmount); 
	}
#endif
}
