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
					if(gun.addMags(magazines)) {
						//gameObject.SetActive(false);
						pickedUp = true;
						handlePickupEffects();
						RpcHandlePickupEffects();
						Destroy(gameObject);
					}
				}
			}
		}
	}

	[ClientRpc]
	private void RpcHandlePickupEffects() {
		handlePickupEffects();
	}

	private void handlePickupEffects() {
		pickupSoundPlayer.spawn(transform.position);
	}
}
