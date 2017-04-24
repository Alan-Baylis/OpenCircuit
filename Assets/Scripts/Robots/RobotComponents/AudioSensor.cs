using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class AudioSensor : AbstractRobotComponent {

	public float range = 30f; 
	private bool hasPower;

	public static HashSet<AudioSensor> sensors = new HashSet<AudioSensor>();

    private List<SoundTag> heardSounds = new List<SoundTag>();

	public void processAudioEvent(LabelHandle soundLabelHandle) {

		if(hasPower && (soundLabelHandle.teamId == null || soundLabelHandle.teamId.Value != getController().GetComponent<TeamId>().id)) {
			heardSounds.Add((SoundTag)soundLabelHandle.getTag(TagEnum.Sound));
			getController().sightingFound(soundLabelHandle, soundLabelHandle.getPosition(), null);
		}
	}

	public float getRange() {
		return range;
	}

	// Use this for initialization
	void Start () {
		if (!sensors.Contains(this)) {
			sensors.Add(this);
		}
	}
	
	// Update is called once per frame
	[ServerCallback]
	void Update () {
		hasPower = powerSource != null && powerSource.hasPower(Time.deltaTime);
	    for (int i = heardSounds.Count-1; i >= 0; --i) {
		    SoundTag soundTag = heardSounds[i];
		    if (soundTag.isExpired()) {
			    getController().sightingLost(soundTag.getLabelHandle(), soundTag.getLabelHandle().getPosition(), soundTag.getLabelHandle().getDirection());
			    heardSounds.RemoveAt(i);
		    }
	    }
	}

	void OnDisable() {
		sensors.Remove(this);
		clearSounds();
	}

	void OnEnable() {
		if (!sensors.Contains(this)) {
			sensors.Add(this);
		}
	}

	private void clearSounds() {
		for (int i = heardSounds.Count-1; i >= 0; --i) {
			SoundTag soundTag = heardSounds[i];
			getController().sightingLost(soundTag.getLabelHandle(), soundTag.getLabelHandle().getPosition(), soundTag.getLabelHandle().getDirection());
			heardSounds.RemoveAt(i);
		}
	}
}
