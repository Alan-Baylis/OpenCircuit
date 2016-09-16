using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public abstract class AbstractArms : AbstractRobotComponent {

	public abstract void dropTarget();
	public abstract void attachTarget(Label obj);
    public abstract Label getTarget();
	public abstract bool hasTarget();
    
	public override System.Type getComponentArchetype() {
		return typeof(AbstractArms);
	}

    [ServerCallback]
    void OnDisable() {
        dropTarget();
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.enabled = false;
    }

    [ServerCallback]
    void OnEnable() {
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.enabled = true;
    }
}
