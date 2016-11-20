using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class InherentEndeavour : Endeavour {

	public InherentEndeavour(EndeavourFactory factory, RobotController controller, List<Goal> goals) : base(factory, controller, goals, new Dictionary<TagEnum, Tag>()) {

	}
}
