using UnityEngine;
using System.Collections;

public class EffectHealth : Health {

	public EffectSpec[] destructEffects;
	public EffectSpec[] hurtEffects;

	public bool destroyOnDestruct = false;

	public override void destruct() {
		base.destruct();
		foreach (EffectSpec effect in destructEffects)
			effect.spawn(transform.position, transform.rotation);
		if(destroyOnDestruct) { 
			//Destroy(gameObject.GetComponent<MeshRenderer>());
			//Destroy(gameObject.GetComponent<ParticleEmitter>());
			gameObject.transform.parent.GetComponent<AudioSource>().Play();
			Destroy(gameObject);
		}
	}

	public override void hurt(float pain) {
		base.hurt(pain);
		foreach (EffectSpec effect in hurtEffects)
			effect.spawn(transform.position, transform.rotation);
	}
}
