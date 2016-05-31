using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioSensor : AbstractRobotComponent {

	public float range = 30f; 
	private bool hasPower;

    public static List<AudioSensor> sensors = new List<AudioSensor>();

	public void processAudioEvent(AudioEvent eventMessage) {
		if(hasPower) {
			//Debug.Log("sound heard");
			getController().enqueueMessage(eventMessage);
		}
	}

	public float getRange() {
		return range;
	}

	// Use this for initialization
	void Start () {
        sensors.Add(this);
	}
	
	// Update is called once per frame
	void Update () {
		hasPower = powerSource != null && powerSource.hasPower(Time.deltaTime);
	}
}
