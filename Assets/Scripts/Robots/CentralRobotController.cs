using System.Collections.Generic;
using UnityEngine;

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
				Debug.LogWarning("Null location attached to CRC with name: " + gameObject.name);
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

#if UNITY_EDITOR
	public bool debug;
	public int sightingsDisplay;
	public int staleSightingsDisplay;
	public List<string> activeSightings =  new List<string>();

	void Update() {
		if (debug) {
			sightingsDisplay = mentalModel.targetSightings.Count;
			staleSightingsDisplay = mentalModel.staleTargetSightings.Count;

			activeSightings.Clear();
			foreach (LabelHandle handle in mentalModel.targetSightings.Keys) {
				activeSightings.Add(handle.getName());
			}
		}
	}
#endif

}