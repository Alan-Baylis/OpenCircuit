using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class InherentEndeavour : Endeavour {

	public InherentEndeavour(EndeavourFactory factory, RobotController controller, List<Goal> goals, LabelHandle parent) : base(factory, controller, goals, parent) {

	}
}
