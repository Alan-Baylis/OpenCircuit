using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Scripts/Robot/Central Robot Controller")]
public class CentralRobotController : MonoBehaviour, MentalModelUpdateListener {

	public RobotController[] robots;
	public Label[] locations;
	
	private List<RobotController> listeners = new List<RobotController>();

	MentalModel mentalModel = new MentalModel();

	// Use this for initialization
	void Start () {
		mentalModel.addUpdateListener (this);
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

	public void notifySighting(LabelHandle target) {
		broadcastMessage (new EventMessage ("target found", target));
	}

	public void notifySightingLost(LabelHandle target) {
		broadcastMessage (new EventMessage ("target lost", target));
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
		listeners.Add(listener);
		listener.attachMentalModel(mentalModel);
	}

	private void broadcastMessage(EventMessage message) {
		for (int i = 0; i < listeners.Count; i++) {
			listeners[i].notify(message);
		}
	}
}