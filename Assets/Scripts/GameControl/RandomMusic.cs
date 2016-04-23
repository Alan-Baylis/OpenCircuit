using UnityEngine;
using System.Collections;

public class RandomMusic : MonoBehaviour {

	private float nextTime;
	private AudioSource source;
	private AudioClip last;
	private bool isActive = false;

	public int minTimeWait;
	public int maxTimeWait;
	public AudioClip[] tracks;

	// Use this for initialization
	void Start () {
		nextTime = Time.fixedTime;// + Random.Range(minTimeWait, maxTimeWait);
		source = gameObject.GetComponent<AudioSource>();
		if (source == null)
			source = gameObject.AddComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		if(isActive) {
			if(nextTime < Time.fixedTime) {
				for(int i = 0; i < 10; ++i) {
					AudioClip newTrack = tracks[Random.Range(0, tracks.Length)];
					if(newTrack != last) {
						last = newTrack;
						break;
					}
				}
				source.clip = last;
				source.Play();
				nextTime = Time.fixedTime + Random.Range(minTimeWait, maxTimeWait) + source.clip.length;
			}
		}
	}

	public void OnTriggerEnter(Collider other) {
		Player player = other.gameObject.GetComponent<Player>();
		if(player != null) {
			isActive = true;
		}
	}
}
