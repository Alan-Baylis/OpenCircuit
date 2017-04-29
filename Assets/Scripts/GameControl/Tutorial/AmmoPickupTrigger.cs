using UnityEngine;

public class AmmoPickupTrigger : MonoBehaviour {
	public OnDamageTrigger odt;
	public Tutorial tutorial;

	private void OnTriggerEnter(Collider other) {
		Player player = other.transform.root.GetComponent<Player>();
		if (player != null && player.isLocalPlayer) {
			tutorial.nextMessage();
			odt.enabled = true;
		}
	}
}
