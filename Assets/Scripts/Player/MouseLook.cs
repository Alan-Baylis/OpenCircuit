using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

/// MouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation

/// To make an FPS style character:
/// - Create a capsule.
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSInputController script to the capsule
///   -> A CharacterMotor and a CharacterController component will be automatically added.

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)
[AddComponentMenu("Scripts/Player/MouseLook")]
public class MouseLook : NetworkBehaviour {

	public float sensitivityX = 1f;
	public float sensitivityY = 1f;

	public float minimumX = -360F;
	public float maximumX = 360F;

	public float minimumY = -60F;
	public float maximumY = 60F;

	private Vector3 lookPoint;
	private bool isAuto = false;
	private float autoLookSpeed = 1f;

	float rotationY = 0F;

	private Transform cam;

	void Start() {
		cam = GetComponentInChildren<Camera>().transform;

		// Make the rigid body not change rotation
		if(GetComponent<Rigidbody>())
			GetComponent<Rigidbody>().freezeRotation = true;
	}

	void Update() {
		if(isAuto) {
			cam.rotation = Quaternion.Lerp(cam.rotation, Quaternion.LookRotation(lookPoint - cam.position), Time.deltaTime * autoLookSpeed);
			if(1 - Mathf.Abs(Vector3.Dot(cam.forward, (lookPoint - cam.position).normalized)) < .05f) {
				isAuto = false;
			}
		}
	}

	public void rotate(float xRotate, float yRotate) {
		float rotationX = cam.localEulerAngles.y + xRotate *sensitivityX;

		rotationY += yRotate *sensitivityY;
		rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

		Vector3 angle = new Vector3(-rotationY, rotationX, 0);
		cam.localEulerAngles = angle;
		CmdSetRotation(angle);

	}

	public void lookAtPoint(Vector3 point, float lookSpeed) {
		lookPoint = point;
		isAuto = true;
		autoLookSpeed = lookSpeed;
	}

	[Command]
	protected void CmdSetRotation(Vector3 eulerAngles) {
		cam.localEulerAngles = eulerAngles;
	}

	public bool isAutoMode() {
		return isAuto;
	}
}