using UnityEngine;
using UnityEngine.Networking;

public class TowerTurret : AbstractRobotGun {

    public GenericRifle riflePrefab;

    public Vector3 recoilAnimationDistance = new Vector3(0, 0, 0.2f);
    private Rotatable horizontalRotatable;
    private Rotatable verticalRotatable;


    [ServerCallback]
    void Start() {
        horizontalRotatable = GetComponent<Rotatable>();
        rifle = Instantiate(riflePrefab, getController().transform.position + riflePrefab.transform.position, riflePrefab.transform.rotation);
        rifle.transform.parent = horizontalRotatable.transform;

        verticalRotatable = rifle.GetComponent<Rotatable>();

        NetworkInstanceId mountId = netId;
        NetworkParenter rifleParenter = rifle.GetComponent<NetworkParenter>();
        rifleParenter.setParentId(mountId);

        NetworkServer.Spawn(rifle.gameObject);
    }

    [Server]
    protected override void trackTarget(Vector3 pos) {
        // Rotate the horizontal rotatable
        Quaternion orig = Quaternion.LookRotation(horizontalRotatable.transform.forward);
        Vector3 delta = pos - horizontalRotatable.transform.position;
        Quaternion look = Quaternion.LookRotation(delta);
        float horizontal = look.eulerAngles.y;
        if (Mathf.Abs(orig.eulerAngles.y - look.eulerAngles.y) > .0001f) {
	        horizontal = Mathf.Abs(orig.eulerAngles.y - horizontal) < horizontalRotatable.rotationSpeed * Time.deltaTime
		        ? horizontal
		        : orig.eulerAngles.y + horizontalRotatable.rotationSpeed * Time.deltaTime*Mathf.Sign(Mathf.DeltaAngle(orig.eulerAngles.y, look.eulerAngles.y));
            horizontalRotatable.transform.rotation = Quaternion.AngleAxis(horizontal, horizontalRotatable.transform.up);
        }

    // Rotate the vertical rotatable
        delta = pos - verticalRotatable.transform.position;
        look = Quaternion.LookRotation(delta);
        float angle = look.eulerAngles.x;
        if (angle > 35f && angle < 90f) {
            angle = 35f;
        }
	    orig = Quaternion.LookRotation(verticalRotatable.transform.forward);
	    angle = Mathf.Abs(orig.eulerAngles.x - angle) < verticalRotatable.rotationSpeed * Time.deltaTime
		    ? angle
		    : orig.eulerAngles.x + verticalRotatable.rotationSpeed * Time.deltaTime *
		      Mathf.Sign(Mathf.DeltaAngle(orig.eulerAngles.x, look.eulerAngles.x));
        verticalRotatable.transform.rotation = Quaternion.AngleAxis(angle, verticalRotatable.transform.right);
        verticalRotatable.transform.rotation = verticalRotatable.transform.rotation*Quaternion.AngleAxis(horizontal, horizontalRotatable.transform.up);
    }
}
