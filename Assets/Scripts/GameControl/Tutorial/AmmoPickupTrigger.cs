using UnityEngine;

public class AmmoPickupTrigger : MonoBehaviour {
	public OnDamageTrigger odt;
	public Tutorial tutorial;

	private void OnTriggerEnter(Collider other) {
		tutorial.nextMessage();
		odt.enabled = true;
	}
}
