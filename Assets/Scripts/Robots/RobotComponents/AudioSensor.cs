using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class AudioSensor : AbstractRobotComponent {

	public float range = 30f; 
	private bool hasPower;

	public static HashSet<AudioSensor> sensors = new HashSet<AudioSensor>();

	public void processAudioEvent(LabelHandle soundLabelHandle) {
		double startTime = Time.realtimeSinceStartup;
		if(hasPower) {
			getController().sightingFound(soundLabelHandle, soundLabelHandle.getPosition(), null);
		}
		double endTime = Time.realtimeSinceStartup;
		getController().getExecutionTimer().addTime(endTime-startTime);
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
