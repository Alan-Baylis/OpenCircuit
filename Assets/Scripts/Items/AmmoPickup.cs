using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class AmmoPickup : NetworkBehaviour {

	public int magazines = 1;
	public AudioClip pickupSound;

	private AudioSource soundEmitter;
	private bool pickedUp = false;

	void Start() {
		soundEmitter = gameObject.AddComponent<AudioSource>();

	}


	[ServerCallback]
	public void OnTriggerEnter(Collider other) {
		if(!pickedUp) {
			Player player = other.GetComponent<Player>();
			if(player != null) {
				AbstractGun gun = player.GetComponentInChildren<AbstractGun>();
				if(gun != null) {
					if(gun.addMags(magazines)) {
						soundEmitter.PlayOneShot(pickupSound);
						//gameObject.SetActive(false);
						pickedUp = true;
						Destroy(GetComponent<MeshRenderer>());
						Destroy(gameObject, 5);
					}
				}
			}
		}
	}
}
