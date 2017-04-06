using UnityEngine;
using UnityEngine.Networking;

public abstract class AbstractRobotGun : AbstractRobotComponent {

    public GenericRifle rifle;

    protected LabelHandle target;

    // Update is called once per frame
    [ServerCallback]
    void Update () {
        if (target != null && rifle.targetInRange(target.getPosition())) {
            rifle.firing = true;
        } else {
            rifle.firing = false;
        }
    }

    [ServerCallback]
    void FixedUpdate() {

        if (target != null) {
            trackTarget(target.getPosition());
        } else {
            trackTarget(transform.position + getController().transform.forward);
        }
    }

    public override void release() {
        target = null;
        rifle.firing = false;
    }

    public void setTarget(LabelHandle handle) {
        target = handle;
    }

    public override System.Type getComponentArchetype() {
        return typeof(AbstractRobotGun);
    }

    protected abstract void trackTarget(Vector3 pos);

}
