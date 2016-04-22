using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Health : NetworkBehaviour {

	public float maxSuffering = 100;
	public float recoveryRate = 25;
	public float sufferingDisplay;

	private AudioSource soundEmitter;
	public AudioClip hurtSound;

	[SyncVar]
	private float suffering = 0;

	void Start() {
		soundEmitter = gameObject.AddComponent<AudioSource>();
		soundEmitter.clip = hurtSound;
	}

	void Update() {
		sufferingDisplay = suffering;
		if(isServer) {
			if(suffering > maxSuffering) {
				// He's dead, Jim.
				destruct();
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

	public virtual void destruct() {
		GetComponent<Label>().sendTrigger(gameObject, new DestructTrigger());
	}

	public virtual void hurt(float pain) {
		suffering += pain;
		if(soundEmitter.clip != null) {
			soundEmitter.Play();
		}
		// play sound or whatever here
	}
}
