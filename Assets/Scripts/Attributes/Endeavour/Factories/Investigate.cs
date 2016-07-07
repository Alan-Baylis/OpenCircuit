using UnityEngine;
using System.Collections;

[System.Serializable]
public class Investigate : InherentEndeavourFactory {

	public override bool isApplicable(LabelHandle labelHandel) {
		return labelHandel.hasTag(TagEnum.Sound);
	}

	public override Endeavour constructEndeavour(RobotController controller, LabelHandle target) {
		return new InvestigateAction(this, controller, this.goals, target);
	}
}
