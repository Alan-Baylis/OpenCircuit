using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class AbstractPowerSource : MonoBehaviour {

	public static List<AbstractPowerSource> powerSources = new List<AbstractPowerSource> ();

	public AudioClip shutDownSound;
	
	private AudioSource soundEmitter;

	protected bool isDisabled = false;

	void Awake() {
		soundEmitter = gameObject.AddComponent<AudioSource>();
		powerSources.Add (this);
	}

	public virtual bool drawPower (float amount) {
		return !isDisabled;
	}

    public abstract bool hasPower(float amount); 

    public void disable(float time = 0) {
		isDisabled = true;
		CancelInvoke ();
		if (Mathf.Abs(time) > .001f) {
			Invoke ("enable", time);
		}
		if(shutDownSound != null) {
			soundEmitter.PlayOneShot(shutDownSound);
		}
	}

	public void enable() {
		isDisabled = false;
	}
}