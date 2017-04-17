using UnityEngine;

public class ParticleEffectController : AbstractEffectController {

	public ParticleSystem[] particleEffects;

	public override void doEffects() {
//		foreach (ParticleSystem particleSystem in particleEffects) {
//			particleSystem.Play();
//		}
	}

	public override bool effectFinished() {
		foreach (ParticleSystem system in particleEffects) {
			if (!system.isStopped) {
				return false;
			}
		}
		return true;
	}
}
