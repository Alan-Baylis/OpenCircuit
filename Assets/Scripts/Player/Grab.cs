using UnityEngine;
using System.Collections;

[AddComponentMenu("Scripts/Player/Grab")]
public class Grab : MonoBehaviour {

	public float strength = 15;
	public float holdDistance = 2f;
	public Vector3 normalHoldPosition = new Vector3(0.0f, -0.5f, 2f);
	private Player myPlayer;
	private Vector3 holdPos;
	private ConfigurableJoint holdJoint;
	private GameObject grabbed = null;
	private JointDrive driver;

	void Awake () {
		myPlayer = this.GetComponent<Player> ();
	}

	public bool grabObject(GameObject obj, Vector3 holdPoint) {
		if (!canGrabObject(obj))
			return false;
		holdObject(obj, holdPoint);
		return true;
	}

	public bool canGrabObject(GameObject obj) {
		if (grabbed != null) {
			return false;
		}
		Rigidbody rb = obj.GetComponent<Rigidbody>();
        return rb != null && !rb.isKinematic;
	}

//	public void pocketObject(GameObject obj) {
//		if (grabbed != null) {
//			if (myPlayer.inventory.isEquiped(grabbed) != null) {
//				GameObject temp = grabbed;
//				releaseObject();
//				GameObject.Destroy(temp);
//			} else if (myPlayer.inventory.canCarry(grabbed)) {
//				GameObject item = grabbed;
//				dropObject();
//				myPlayer.inventory.addItem(item);
//			}
//			return;
//		}
//		if (obj != null)
//			this.GetComponent<Inventory>().addItem(obj);
//	}

	public void dropObject() {
		if (grabbed == null)
			return;

		releaseObject();
	}

	private void releaseObject() {
		if (grabbed == null)
			return;
		Destroy (holdJoint);
		Physics.IgnoreCollision(GetComponent<Collider>(), grabbed.GetComponent<Collider>(), false);
		grabbed = null;
	}

	protected void holdObject(GameObject obj, Vector3 holdPoint) {
		grabbed = obj;

		Physics.IgnoreCollision(GetComponent<Collider>(), grabbed.GetComponent<Collider>(), true);

		// create and configure the joint
		driver = new JointDrive(); 
		driver.positionDamper = strength /5;
		driver.positionSpring = strength *3f;
		driver.maximumForce = strength *3;

		holdJoint = grabbed.AddComponent<ConfigurableJoint>();
		holdJoint.autoConfigureConnectedAnchor = false;
		holdJoint.rotationDriveMode = RotationDriveMode.Slerp;
		//holdJoint.breakForce = 15f;

		holdJoint.connectedBody = this.gameObject.transform.GetChild(0).GetComponent<Rigidbody>();
		holdJoint.connectedAnchor = normalHoldPosition;
		if ((grabbed.GetComponent<Rigidbody>().worldCenterOfMass -holdPoint).sqrMagnitude > holdDistance)
			holdJoint.anchor = grabbed.transform.InverseTransformPoint(holdPoint);
		else
			holdJoint.anchor = grabbed.GetComponent<Rigidbody>().centerOfMass;
		holdJoint.projectionDistance = 0;
		holdJoint.projectionAngle = 0;
		holdJoint.xDrive = holdJoint.yDrive = holdJoint.zDrive = holdJoint.slerpDrive = driver;
		holdJoint.targetPosition = Vector3.zero;
	}

	public bool hasObject() {
		return (grabbed != null);
	}

	public Vector3 holdPosition() {
		return myPlayer.cam.transform.TransformPoint(normalHoldPosition);
	}

	public GameObject getCarried() {
		return grabbed;
	}

	public void holdAside(bool aside) {
		if (grabbed == null)
			return;
		if (aside)
			holdJoint.connectedAnchor = normalHoldPosition +new Vector3(0, -0.5f, -holdDistance *0.2f);
		else
			holdJoint.connectedAnchor = normalHoldPosition;
	}

}
