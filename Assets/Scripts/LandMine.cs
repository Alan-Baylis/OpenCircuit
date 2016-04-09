using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LandMine : NetworkBehaviour {

	public float knockBack = 5f;
	public float damage = 70f;
	public float blackoutTime = 2;
    public AudioClip explosionSound;
	public EffectSpec explosion;
	public EffectSpec explosionLight;

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

				// move player
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
		Player player = ClientScene.FindLocalObject(id).GetComponent<Player>();
		handleEffects(player);

		// move player screen and do blackout
		player.looker.rotate(Random.Range(-45, 45), Random.Range(-30, 30));
		player.blackout(blackoutTime);
		if (explosionSound != null) {
			soundEmitter.PlayOneShot(explosionSound);
		}
		explosion.spawn(transform.position);
		explosionLight.spawn(transform.position +Vector3.up);
	}

	private void handleEffects(Player player) {
		Destroy(GetComponent<MeshRenderer>());
		Destroy(GetComponent<MeshFilter>());
	}
}
