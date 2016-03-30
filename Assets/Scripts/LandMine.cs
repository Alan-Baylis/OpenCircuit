using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LandMine : NetworkBehaviour {

	public float knockBack = 5f;
	public float damage = 70f;
	public AudioClip explosionSound;

	private AudioSource soundEmitter;
	private bool triggered = false;

	void Start() {
		soundEmitter = gameObject.AddComponent<AudioSource>();
		soundEmitter.volume = 1f;
	}

	[ServerCallback]
	public void OnTriggerEnter(Collider other) {
		if(!triggered) {
			Player player = other.GetComponent<Player>();
			if(player != null) {
				triggered = true;

				player.GetComponent<Label>().sendTrigger(this.gameObject, new DamageTrigger(damage));
				Vector3 horizontalDirection = (player.transform.position - transform.position);
				Vector3 verticalDirection = new Vector3(0, 1, 0);
				horizontalDirection.y = 0;
				horizontalDirection.Normalize();
				player.GetComponent<Rigidbody>().AddForce(horizontalDirection * knockBack + verticalDirection * (knockBack * .3f));
				handleEffects(player);
				RpcHandleEffects(player.netId);
				Destroy(gameObject, 3f);

			}
		}
	}

	[ClientRpc]
	private void RpcHandleEffects(NetworkInstanceId id) {
		handleEffects(ClientScene.FindLocalObject(id).GetComponent<Player>());
	}

	private void handleEffects(Player player) {
		if(explosionSound != null) {
			soundEmitter.PlayOneShot(explosionSound);
		}
		Destroy(GetComponent<MeshRenderer>());
		Destroy(GetComponent<MeshFilter>());
	}
}
