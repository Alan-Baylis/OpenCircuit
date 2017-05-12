using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class AmmoPickup : NetworkBehaviour {

	public int magazines = 1;
	public EffectSpec pickupSoundPlayer;

	[SyncVar]
	private bool pickedUp = false;


	[ServerCallback]
	public void OnTriggerEnter(Collider other) {
		if(!pickedUp) {
			Player player = other.GetComponent<Player>();
			if(player != null) {
				AbstractGun gun = player.GetComponentInChildren<AbstractGun>();
				if(gun != null) {
					if(gun.addAmmo(magazines)) {
						//gameObject.SetActive(false);
						pickedUp = true;
						Destroy(gameObject);
					}
				}
			}
		}
	}

	[ClientCallback]
	void OnDestroy() {
		handlePickupEffects();
	}

	private void handlePickupEffects() {
		pickupSoundPlayer.spawn(transform.position);
	}
}
