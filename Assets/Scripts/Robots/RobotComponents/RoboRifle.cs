using UnityEngine;
using UnityEngine.Networking;

public class RoboRifle : AbstractRobotComponent {

    public GenericRifle riflePrefab;
    public Rotatable elbowPrefab;

    public Vector3 recoilAnimationDistance = new Vector3(0, 0, 0.2f);
    public Rotatable elbow;
    public GenericRifle rifle;

    private LabelHandle target;

    [ServerCallback]
    void Start() {
        rifle = Instantiate(riflePrefab, getController().transform.position + riflePrefab.transform.position, riflePrefab.transform.rotation);
        elbow = Instantiate(elbowPrefab, getController().transform.position + elbowPrefab.transform.position, elbowPrefab.transform.rotation);

	    rifle.transform.parent = elbow.transform;
        elbow.transform.parent = transform;

	    NetworkInstanceId elbowId = elbow.GetComponent<NetworkIdentity>().netId;
        NetworkInstanceId mountId = netId;

	    NetworkParenter elbowParenteer = elbow.GetComponent<NetworkParenter>();
        elbowParenteer.setParentId(mountId);

	    NetworkParenter rifleParenter = rifle.GetComponent<NetworkParenter>();
        rifleParenter.setParentId(elbowId);

        NetworkServer.Spawn(rifle.gameObject);
        NetworkServer.Spawn(elbow.gameObject);
    }

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
            trackTarget(transform.position - transform.forward);
        }
    }

    [Server]
    private void trackTarget(Vector3 pos) {
        elbow.transform.rotation = Quaternion.RotateTowards(elbow.transform.rotation, Quaternion.LookRotation(pos - elbow.transform.position), elbow.rotationSpeed * Time.deltaTime);
    }

    public override void release() {
        target = null;
        rifle.firing = false;
    }

    public void setTarget(LabelHandle handle) {
        target = handle;
    }
}
