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
		soundEmitter.spatialBlend = 1f;
	}

	[ServerCallback]
	public void OnTriggerEnter(Collider other) {
		Player player = other.GetComponent<Player>();
		if (player != null)
			detonate(player);
	}

	[Server]
	public void detonate() {
		detonate(null);
	}

	[Server]
	public void detonate(Player player) {
		if(!triggered) {
			triggered = true;
			if (player != null) {

				player.GetComponent<Label>().sendTrigger(this.gameObject, new DamageTrigger(damage));

				// move player
				Vector3 horizontalDirection = (player.transform.position - transform.position);
				Vector3 verticalDirection = new Vector3(0, 1, 0);
				horizontalDirection.y = 0;
				horizontalDirection.Normalize();
				player.GetComponent<Rigidbody>().AddForce(horizontalDirection * knockBack + verticalDirection * (knockBack * .3f));
				RpcHandleEffects(player.netId);
			} else {
				RpcHandleEffects(NetworkInstanceId.Invalid);
			}
			
			Destroy(gameObject, 3f);
		}
	}

	[ClientRpc]
	private void RpcHandleEffects(NetworkInstanceId id) {
		Destroy(GetComponent<MeshRenderer>());
		Destroy(GetComponent<MeshFilter>());

		// move player screen and do blackout
		if (id != NetworkInstanceId.Invalid) {
			Player player = ClientScene.FindLocalObject(id).GetComponent<Player>();
			player.looker.rotate(Random.Range(-45, 45), Random.Range(-30, 30));
			player.blackout(blackoutTime);
		}

		// do effects
		if (explosionSound != null) {
			soundEmitter.PlayOneShot(explosionSound);
		}
		explosion.spawn(transform.position);
		explosionLight.spawn(transform.position +Vector3.up);
	}
}
