using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class AmmoPickup : NetworkBehaviour {

	public int magazines = 1;

	[ServerCallback]
	public void OnTriggerEnter(Collider other) {
		Player player = other.GetComponent<Player>();
		if(player != null) {
			AbstractGun gun = player.GetComponentInChildren<AbstractGun>();
			if(gun != null) {
				if(gun.addMags(magazines)) {
					Destroy(gameObject);
				}
			}
		}
	}
}
