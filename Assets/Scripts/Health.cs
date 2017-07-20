using UnityEngine;
using UnityEngine.Networking;

public class Health : NetworkBehaviour {

	public float maxSuffering = 100;
	public float recoveryRate = 25;
	public float sufferingDisplay;

	private AudioSource soundEmitter;
	public AudioClip hurtSound;
	public float hurtSoundPitch = 1;

	public Vector3 lastAttackPosition;
	private GameObject lastAttacker;
	private Label label;

	[SyncVar]
	private float suffering = 0;

	void Start() {
		label = GetComponent<Label>();
		if (label != null) {
			label.setTag(new Tag(TagEnum.Health, 0, label.labelHandle));
			label.addOperation(new DamageOperation(this), new [] { typeof(DamageTrigger) });
		}

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

	[Server]
	public virtual void destruct() {
		if (label != null)
			label.sendTrigger(lastAttacker, new DestructTrigger());
	}

	public virtual void hurt(float pain, GameObject instigator) {
		lastAttacker = instigator;
		lastAttackPosition = (transform.position - instigator.transform.position).normalized;
		suffering += pain;
		if(soundEmitter.clip != null) {
			soundEmitter.pitch = Random.Range(hurtSoundPitch -0.05f, hurtSoundPitch +0.05f);
			soundEmitter.Play();
		}
		// play sound or whatever here
	}
}
