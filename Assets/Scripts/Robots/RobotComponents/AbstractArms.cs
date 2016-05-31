using UnityEngine;
using System.Collections;

public abstract class AbstractArms : AbstractRobotComponent {

	public abstract void dropTarget();
	public abstract void attachTarget(Label obj);
	public abstract Label getProposedTarget();
	public abstract bool hasTarget();



	public override System.Type getComponentArchetype() {
		return typeof(AbstractArms);
	}
}
