using UnityEngine;
using System.Collections;

[System.Serializable]
public class Search : EndeavourFactory {

	public override Endeavour constructEndeavour(RobotController controller) {
		if (parent == null) {
			return null;
		}
		return new SearchAction(this, controller, goals, parent.labelHandle);
	}
}
