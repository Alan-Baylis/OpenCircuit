using UnityEngine;
using UnityEngine.Networking;

public class RoboRifle : AbstractRobotGun {

    public GenericRifle riflePrefab;
    public Rotatable elbowPrefab;

    public Vector3 recoilAnimationDistance = new Vector3(0, 0, 0.2f);
    public Rotatable elbow;


    [ServerCallback]
    void Start() {
        rifle = Instantiate(riflePrefab, getController().transform.position + riflePrefab.transform.position, riflePrefab.transform.rotation);
        elbow = Instantiate(elbowPrefab, getController().transform.position + elbowPrefab.transform.position, elbowPrefab.transform.rotation);

	    rifle.transform.parent = elbow.transform;
        elbow.transform.parent = transform;

	    // Spawn the elbow - this has to happern first so that the elbow's netId gets set
        NetworkInstanceId mountId = netId;

	    NetworkParenter elbowParenteer = elbow.GetComponent<NetworkParenter>();
        elbowParenteer.setParentId(mountId);


		// Spawn the rifle
        NetworkServer.Spawn(elbow.gameObject);
	    NetworkInstanceId elbowId = elbow.GetComponent<NetworkIdentity>().netId;
	    NetworkParenter rifleParenter = rifle.GetComponent<NetworkParenter>();
	    rifleParenter.setParentId(elbowId);
	    NetworkServer.Spawn(rifle.gameObject);
    }

    [Server]
    protected override void trackTarget(Vector3 pos) {
        elbow.transform.rotation = Quaternion.RotateTowards(elbow.transform.rotation, Quaternion.LookRotation(pos - elbow.transform.position), elbow.rotationSpeed * Time.deltaTime);
    }
}
