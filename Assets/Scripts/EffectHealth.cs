using UnityEngine;
using UnityEngine.Networking;

public class EffectHealth : Health {

	public EffectSpec[] destructEffects;
	public EffectSpec[] hurtEffects;

	public bool destroyOnDestruct = false;

	[Server]
	public override void destruct() {
		base.destruct();
		RpcSpawnEffects();
		if(destroyOnDestruct) { 
			//Destroy(gameObject.GetComponent<MeshRenderer>());
			//Destroy(gameObject.GetComponent<ParticleEmitter>());
			Destroy(gameObject);
		}
	}

	public override void hurt(float pain) {
		base.hurt(pain);
		foreach (EffectSpec effect in hurtEffects)
			effect.spawn(transform.position, transform.rotation);
	}

	[ClientRpc]
	private void RpcSpawnEffects() {
				foreach (EffectSpec effect in destructEffects)
			effect.spawn(transform.position, transform.rotation);
	}
}
