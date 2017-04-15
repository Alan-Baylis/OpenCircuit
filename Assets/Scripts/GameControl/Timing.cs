using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timing : MonoBehaviour {
	public double timeSinceLastUpdate;
	public double timePerSecond;
	public double measuredTime;
	public double measuredTimePeriod;

	private double lastReset;
	private double aggregateTime;
	private double updateStartTime;
	private double resetInterval = 1.0f;

	// Use this for initialization
	void Start() {
		lastReset = Time.realtimeSinceStartup;
	}

	// Update is called once per frame
	void Update() {
		timeSinceLastUpdate = 0;
	}

	public void addTime(double timeToAdd) {
		timeSinceLastUpdate += timeToAdd;
	}

	public double getMeasuredTimePerSecond() {
		return timePerSecond;
	}

	public double getEstimatedTimePerSecond() {
		return aggregateTime / (Time.realtimeSinceStartup - lastReset);
	}

	public void setResetInterval(double interval) {
		resetInterval = interval;
	}

	void LateUpdate() {
		double timeDiff = Time.realtimeSinceStartup - lastReset;
		if (timeDiff > resetInterval) {
			timePerSecond = aggregateTime / timeDiff;
			measuredTime = aggregateTime;
			measuredTimePeriod = timeDiff;
			aggregateTime = timeSinceLastUpdate;
			lastReset = Time.realtimeSinceStartup;
		}
		else {
			aggregateTime += timeSinceLastUpdate;
		}
	}
}
