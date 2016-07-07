using UnityEngine;
using System.Collections;

[System.Serializable]
public class Pursue : EndeavourFactory {

	public override Endeavour constructEndeavour (RobotController controller) {
		if (parent == null) {
			return null;
		}
		//Goal[] goals = new Goal[2];
		//goals [0] = new Goal ("protection", 3);
		//goals [1] = new Goal ("offense", 3);
		return new PursueAction(this, controller, goals, parent);
	}
}
