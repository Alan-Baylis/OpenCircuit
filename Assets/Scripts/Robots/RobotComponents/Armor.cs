using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Armor : AbstractRobotComponent {

	public float plateSlowingMultiplier = 0.9f;

	private HashSet<ArmorPlate> plates = new HashSet<ArmorPlate>();

	[ServerCallback]
	public void Start() {
		foreach(ArmorPlate plate in GetComponentsInChildren<ArmorPlate>()) {
			plates.Add(plate);
			plate.setArmor(this);
		}
		calculateSpeedMultiplier();
	}

	[Server]
	public void plateDestroyed(ArmorPlate plate) {
		if (plates.Remove(plate))
			calculateSpeedMultiplier();
	}

	private void calculateSpeedMultiplier() {
		HoverJet jet = getController().getRobotComponent<HoverJet>();
		if (jet == null) {
			Debug.LogWarning("Failed to apply robot slow effect from armor plates: " +name);
		} else {
			jet.speedMultipler = Mathf.Pow(plateSlowingMultiplier, plates.Count);
        }
	}
}
