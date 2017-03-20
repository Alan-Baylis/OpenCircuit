using UnityEngine;
using UnityEngine.Networking;

public class RoboRifle : AbstractRobotComponent {

    public Vector3 recoilAnimationDistance = new Vector3(0, 0, 0.2f);
    public Rotatable rotatable;
    public GenericRifle rifle;

    private LabelHandle target;

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

        if (target != null) { //no point if there are no targets in view
            trackTarget(target.getPosition());
        }
    }

    [Server]
    private void trackTarget(Vector3 pos) {
        rotatable.transform.rotation = Quaternion.RotateTowards(rotatable.transform.rotation, Quaternion.LookRotation(pos - rotatable.transform.position), rotatable.rotationSpeed * Time.deltaTime);
    }

    public override void release() {
        target = null;
        rotatable.transform.rotation = Quaternion.LookRotation(transform.forward);
    }

    public void setTarget(LabelHandle handle) {
        target = handle;
    }
}
