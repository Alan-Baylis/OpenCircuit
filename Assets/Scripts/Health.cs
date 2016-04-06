using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Health : NetworkBehaviour {

	public float maxSuffering = 100;
	public float recoveryRate = 25;
	public float sufferingDisplay;

	[SyncVar]
	private float suffering = 0;

	void Update() {
		sufferingDisplay = suffering;
		if(isServer) {
			if(suffering > maxSuffering) {
				// He's dead, Jim.
				GetComponent<Label>().sendTrigger(gameObject, new DestructTrigger());
			}
		}

		if(suffering > 0)
			suffering = Mathf.Max(suffering - recoveryRate * Time.deltaTime, 0f);
	}

	public float getDamagePercent() {
		return Mathf.Min(suffering, maxSuffering) / maxSuffering;
	}

	public float getDamage() {
		return suffering;
	}

	public void hurt(float pain) {
		suffering += pain;
		// play sound or whatever here
	}
}
