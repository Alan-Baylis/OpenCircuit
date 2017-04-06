using UnityEngine;
using UnityEngine.Networking;

public class RoboRifle : AbstractRobotGun {

    public GenericRifle riflePrefab;
    public Rotatable elbowPrefab;

    public Vector3 recoilAnimationDistance = new Vector3(0, 0, 0.2f);
    public Rotatable rotatable;


    [ServerCallback]
    void Start() {
        rifle = Instantiate(riflePrefab, getController().transform.position + riflePrefab.transform.position, riflePrefab.transform.rotation);
        rotatable = Instantiate(elbowPrefab, getController().transform.position + elbowPrefab.transform.position, elbowPrefab.transform.rotation);

        rifle.transform.parent = rotatable.transform;
        rotatable.transform.parent = transform;

	    // Spawn the elbow - this has to happern first so that the elbow's netId gets set
        NetworkInstanceId mountId = netId;
        NetworkParenter elbowParenteer = rotatable.GetComponent<NetworkParenter>();
        elbowParenteer.setParentId(mountId);
	    NetworkServer.Spawn(rotatable.gameObject);

		// Spawn the rifle
	    NetworkInstanceId elbowId = rotatable.GetComponent<NetworkIdentity>().netId;
	    NetworkParenter rifleParenter = rifle.GetComponent<NetworkParenter>();
	    rifleParenter.setParentId(elbowId);
	    NetworkServer.Spawn(rifle.gameObject);
    }

    [Server]
    protected override void trackTarget(Vector3 pos) {
        rotatable.transform.rotation = Quaternion.RotateTowards(rotatable.transform.rotation, Quaternion.LookRotation(pos - rotatable.transform.position), rotatable.rotationSpeed * Time.deltaTime);
    }
}
