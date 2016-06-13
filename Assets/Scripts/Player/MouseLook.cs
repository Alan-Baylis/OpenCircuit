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
	public float zoomRate = 0.5f;

	private Vector3 lookPoint;
	private bool isAuto = false;
	private float autoLookSpeed = 1f;
	public float defaultFov;
	private float currentZoom = 1;

	private float rotationY = 0F;

	private Player myPlayer;

	public Player player {
		get {
			if(myPlayer == null) {
				myPlayer = GetComponent<Player>();
			}
			return myPlayer; }
	}

	void Start() {
		// Make the rigid body not change rotation
		if(GetComponent<Rigidbody>())
			GetComponent<Rigidbody>().freezeRotation = true;
	}

	void Update() {
		// do zooming
		player.cam.fieldOfView += (defaultFov /currentZoom -player.cam.fieldOfView) *zoomRate;

		if(isAuto) {
			player.cam.transform.rotation = Quaternion.Lerp(player.cam.transform.rotation, Quaternion.LookRotation(lookPoint - player.cam.transform.position), Time.deltaTime * autoLookSpeed);
			if(1 - Mathf.Abs(Vector3.Dot(player.cam.transform.forward, (lookPoint - player.cam.transform.position).normalized)) < .05f) {
				isAuto = false;
			}
		}
	}

	public void rotate(float xRotate, float yRotate) {
		float rotationX = player.cam.transform.localEulerAngles.y + xRotate * sensitivityX;

		rotationY += yRotate *sensitivityY;
		rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

		Vector3 angle = new Vector3(-rotationY, rotationX, 0);
		player.cam.transform.localEulerAngles = angle;
		CmdSetRotation(angle);

	}

	public void lookAtPoint(Vector3 point, float lookSpeed) {
		lookPoint = point;
		isAuto = true;
		autoLookSpeed = lookSpeed;
	}

	public void resetCameraZoom() {
		setCameraZoom(-1);
	}

	public void setCameraZoom(float zoom) {
		this.currentZoom = zoom <= 0? 1 : zoom;
	}

	[Command]
	protected void CmdSetRotation(Vector3 eulerAngles) {
		player.cam.transform.localEulerAngles = eulerAngles;
	}

	public bool isAutoMode() {
		return isAuto;
	}
}