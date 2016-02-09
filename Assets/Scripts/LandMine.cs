using UnityEngine;
using System.Collections;

public class LandMine : MonoBehaviour {

	public float knockBack = 5f;
	public float damage = 70f;
	public AudioClip explosionSound;

	private AudioSource soundEmitter;

	void Start() {
		soundEmitter = gameObject.AddComponent<AudioSource>();
		soundEmitter.volume = 1f;
	}

	public void OnTriggerEnter(Collider other) {
		Player player = other.GetComponent<Player>();
		if(player != null) {
			if(explosionSound != null) {
				soundEmitter.PlayOneShot(explosionSound);
			}
			player.GetComponent<Label>().sendTrigger(this.gameObject, new DamageTrigger(damage));
			Vector3 horizontalDirection = (player.transform.position - transform.position);
			Vector3 verticalDirection = new Vector3(0, 1, 0);
			horizontalDirection.y = 0;
			horizontalDirection.Normalize();
			player.GetComponent<Rigidbody>().AddForce(horizontalDirection * knockBack + verticalDirection*(knockBack*.3f));
			Destroy(GetComponent<MeshRenderer>());
			Destroy(GetComponent<MeshFilter>());
			Destroy(gameObject, 3f);

		}
	}
}
