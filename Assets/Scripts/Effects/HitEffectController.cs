using UnityEngine;

public class HitEffectController : ParticleEffectController {

	public AudioSource hitSoundSource;

	public override void doEffects() {
		base.doEffects();
		hitSoundSource.Play();
	}

	public override bool effectFinished() {
		return base.effectFinished() && !hitSoundSource.isPlaying;
	}
}
