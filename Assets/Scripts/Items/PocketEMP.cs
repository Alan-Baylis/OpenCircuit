using UnityEngine;
using System.Collections;

[AddComponentMenu("Scripts/Items/Pocket EMP")]
public class PocketEMP : RechargableItem {

	public float range = 3;
	public float disableTime = 5;

	protected AudioSource audioSource;

	public void Start() {
		audioSource = GetComponent<AudioSource>();
		if (audioSource == null) {
			audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.spatialBlend = 1;
		}
    }

	public override void beginInvoke(Inventory invoker) {
		//audioSource.Play();
		if(useCharge()) {
			foreach(AbstractPowerSource source in AbstractPowerSource.powerSources) {
				if(Vector3.Distance(invoker.transform.position, source.transform.position) < range)
					source.disable(disableTime);
			}
		}
		
	}
}
