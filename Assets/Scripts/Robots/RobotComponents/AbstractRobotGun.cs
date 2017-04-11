using UnityEngine;
using UnityEngine.Networking;

public abstract class AbstractRobotGun : AbstractRobotComponent {

    public GenericRifle rifle;

    protected LabelHandle currentTarget;

	public LabelHandle target {
		get { return currentTarget; }
	}

    // Update is called once per frame
    [ServerCallback]
    void Update () {
        if (currentTarget != null && rifle.targetInRange(currentTarget.getPosition())) {
            rifle.firing = true;
        } else {
            rifle.firing = false;
        }
    }

    [ServerCallback]
    void FixedUpdate() {

        if (currentTarget != null) {
            trackTarget(currentTarget.getPosition());
        } else {
            trackTarget(transform.position + getController().transform.forward);
        }
    }

    public override void release() {
        currentTarget = null;
        rifle.firing = false;
    }

    public void setTarget(LabelHandle handle) {
        currentTarget = handle;
    }

	public bool targetObstructed(LabelHandle handle) {
		Vector3 objPos = handle.getPosition();
		bool result = true;
		RaycastHit hit;
		Vector3 fireEffectLocation = rifle.worldFireEffectLocation;
		Vector3 dir = objPos - fireEffectLocation;
		dir.Normalize();
		Physics.Raycast(fireEffectLocation, dir, out hit, rifle.range);
		if (hit.transform == handle.label.transform || hit.transform.root == handle.label.transform) {//&& Vector3.Dot (transform.forward.normalized, (objPos - eye.transform.position).normalized) > 0) {
			result = false;
		}
		return result;
	}

    public override System.Type getComponentArchetype() {
        return typeof(AbstractRobotGun);
    }

    protected abstract void trackTarget(Vector3 pos);

}
