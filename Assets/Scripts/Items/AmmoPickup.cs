using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class AmmoPickup : NetworkBehaviour {

	public int magazines = 1;
	public AudioClip pickupSound;

	private AudioSource soundEmitter;

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
						Destroy(gameObject, 5);
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
		getAudioSource().PlayOneShot(pickupSound);
		Destroy(GetComponent<MeshRenderer>());
	}

	protected AudioSource getAudioSource() {
		if(soundEmitter == null) {
			soundEmitter = GetComponent<AudioSource>();
		}
		return soundEmitter;
	}
}
