using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Scripts/Items/Rock")]
public class Rock : MonoBehaviour {

	public static int maxRocks = 10;

	public float volume = 0.1f;
	public float volumeThreshold = 0.1f;
	public AudioClip clip;

	protected AudioSource source;

	private static List<Rock> rocks = new List<Rock>(maxRocks);

	public void Start () {
		rocks.Add(this);
		while(rocks.Count > maxRocks) {
			GameObject.Destroy(rocks[0].gameObject);
			rocks.RemoveAt(0);
		}

		source = GetComponent<AudioSource>();
		if (source == null)
			source = gameObject.AddComponent<AudioSource>();
		source.maxDistance = 30;
		source.spatialBlend = 1;
	}

	public void OnDestroy() {
		rocks.Remove(this);
	}

	public void OnCollisionEnter(Collision col) {
		float volume = col.impulse.magnitude *this.volume;
		if (volume > volumeThreshold) {
			source.volume = volume;
			if (source.clip == null)
				source.clip = clip;
			source.Play(0);
			LabelHandle audioLabel = new LabelHandle(transform.position, "rock");
			audioLabel.addTag(new Tag(TagEnum.Sound, volume, audioLabel));
			audioLabel.addTag(new Tag(TagEnum.Threat, 5f, audioLabel));
			AudioEvent rockCollisionSound = new AudioEvent(transform.position, audioLabel, transform.position);
			rockCollisionSound.broadcast(volume);

		}
	}
}
