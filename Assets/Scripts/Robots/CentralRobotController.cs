using UnityEngine;
using System.Collections.Generic;
using System;

[AddComponentMenu("Scripts/Robot/Central Robot Controller")]
public class CentralRobotController : MonoBehaviour {

	public RobotController[] robots;
	public Label[] locations;

	MentalModel mentalModel = new MentalModel();

	// Use this for initialization
	void Start () {
		for (int i = 0; i < robots.Length; i++) {
			if (robots[i] == null) {
					Debug.LogWarning("Null robot attached to CRC with name: " + gameObject.name);
					continue;
			}
			addListener(robots[i]);
		}
		foreach (Label location in locations) {
			if (location == null) {
				if (location == null) {
					Debug.LogWarning("Null location attached to CRC with name: " + gameObject.name);
					continue;
				}
			} else {
				mentalModel.addSighting(location.labelHandle, location.transform.position, null);
			}
		}
	}

	public void addListener(RobotController listener) {
		RobotAntenna antenna = listener.getRobotComponent<RobotAntenna>();
		if (antenna != null) {
			forceAddListener(listener);
		} else {
			Debug.LogWarning("Cannot add robot without antenna to CRC: " + listener.name);
		}
	}

	public void forceAddListener(RobotController listener) {
		listener.attachMentalModel(mentalModel);
	}

	public void sightingFound(LabelHandle target, Vector3 pos, Vector3? dir) {
		mentalModel.addSighting(target, pos, dir);
	}
}