using UnityEngine;

public class SoundTag : Tag {

	private float timeStamp;
	private float expirationTime;

	public SoundTag(TagEnum type, float severity, LabelHandle handle, float time, float expirationPeriod) : base(type, severity, handle) {
		timeStamp = time;
		expirationTime = expirationPeriod;
	}

	public bool isExpired() {
		return Time.time - timeStamp >= expirationTime;
	}
}
