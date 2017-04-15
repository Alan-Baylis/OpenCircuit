using System.Collections;
using UnityEngine;

public class GunEffectsController : MonoBehaviour {
	public Light flash;
	public float flashDuration = 0.03f;

	public ParticleSystem[] particleEffects;

	public void doEffects() {
		foreach (ParticleSystem particleSystem in particleEffects) {
			particleSystem.Play();
		}
		flash.enabled = true;
		StartCoroutine("stopFlashAfterDuration");
	}

	IEnumerator stopFlashAfterDuration() {
		yield return new WaitForSeconds(flashDuration);
		flash.enabled = false;
	}
}
