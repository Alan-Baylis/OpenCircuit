using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class AudioSensor : AbstractRobotComponent {

	public float range = 30f; 
	private bool hasPower;

    public static List<AudioSensor> sensors = new List<AudioSensor>();

	public void processAudioEvent(AudioEvent eventMessage) {
		if(hasPower) {
			getController().enqueueMessage(eventMessage);
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
	}

    void OnDisable() {
        sensors.Remove(this);
    }

    void OnEnable() {
        if (!sensors.Contains(this)) {
            sensors.Add(this);
        }
    }
}
