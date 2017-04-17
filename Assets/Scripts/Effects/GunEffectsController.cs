using System.Collections;
using UnityEngine;

public class GunEffectsController : ParticleEffectController {
	public Light flash;
	public float flashDuration = 0.03f;


	public override void doEffects() {
		base.doEffects();
		flash.enabled = true;
		StartCoroutine("stopFlashAfterDuration");
	}

	public override bool effectFinished() {
		return base.effectFinished() && !flash.enabled;
	}

	IEnumerator stopFlashAfterDuration() {
		yield return new WaitForSeconds(flashDuration);
		flash.enabled = false;
	}
}
