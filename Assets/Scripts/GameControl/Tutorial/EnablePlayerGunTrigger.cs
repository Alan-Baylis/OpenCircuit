using UnityEngine;

public class EnablePlayerGunTrigger : MonoBehaviour {

	public Tutorial tutorial;

	private void OnTriggerEnter(Collider other) {
		Player player = other.transform.root.GetComponent<Player>();
		if (player != null && player.isLocalPlayer) {
			tutorial.nextMessage();
			AssaultRifle rifle = (AssaultRifle)player.inventory.getItemsExtending<AbstractGun>()[0];
			rifle.clearAmmo();
			player.inventory.equip(rifle);
		}
	}
}
