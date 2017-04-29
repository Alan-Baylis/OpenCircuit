using UnityEngine;

public class ArmsController : MonoBehaviour {

	public float minRotationDiff;
	public float rotationPercent;
	public float maxDegreesPerSecond;
	public float minDegreesPerSecond;

	public float maxHandMoveSpeed = 5;
	public float minHandMoveSpeed = 1;
	public float minHandMoveUrgency = 0.2f;

	protected bool rotating;

	private Player myPlayer;
	private ArmPoser myRightArm;
	private ArmPoser myLeftArm;

	protected Player player {
		get { return myPlayer ?? (myPlayer = transform.GetComponentInParent<Player>()); }
	}

	protected ArmPoser rightArm {
		get { return myRightArm ?? (myRightArm = transform.FindChild("Right Arm").GetComponent<ArmPoser>()); }
	}
	protected ArmPoser leftArm {
		get { return myLeftArm ?? (myLeftArm = transform.FindChild("Left Arm").GetComponent<ArmPoser>()); }
	}


	public void LateUpdate () {
		updateRotation();

		// update hand positions
		updateHandPosition(rightArm, getRightHandTarget());
		updateHandPosition(leftArm, getLeftHandTarget());
	}

	private HandTarget getRightHandTarget() {
		Item equipped = player.inventory.getEquipped();
		if (equipped == null)
			return new HandTarget(rightArm.getDefaultPos(), minHandMoveUrgency);
		return new HandTarget(equipped.transform.TransformPoint(equipped.grabPosition), 1f);
	}

	private HandTarget getLeftHandTarget() {
		Item equipped = player.inventory.getEquipped();
		if (equipped == null || !equipped.twoHanded)
			return new HandTarget(leftArm.getDefaultPos(), minHandMoveUrgency);
		return new HandTarget(equipped.transform.TransformPoint(equipped.secondaryGrabPosition), 1f);
	}

	private void updateHandPosition(ArmPoser arm, HandTarget target) {
		Vector3 current = arm.deducePosition();
		Vector3 diff = target.target - current;
		float distance = diff.magnitude;
		float percentage = Mathf.Min(Mathf.Max(Mathf.Min(target.urgency, maxHandMoveSpeed / distance), minHandMoveSpeed / distance), 1);
		arm.setPosition(current +diff *percentage);
	}

	private void updateRotation() {
		Quaternion camRotation = Quaternion.Euler(0, player.head.transform.localEulerAngles.y, 0);
		Quaternion rotation = Quaternion.Euler(transform.localEulerAngles);
		float diff = Quaternion.Angle(rotation, camRotation);
		if (rotating) {
			float rotationChange = Mathf.Max(minDegreesPerSecond *Time.deltaTime,
				Mathf.Min(diff * rotationPercent, maxDegreesPerSecond * Time.deltaTime));
			transform.localEulerAngles = Quaternion.RotateTowards(rotation, camRotation, rotationChange).eulerAngles;
			if (diff < rotationChange)
				rotating = false;
		} else {
			if (diff > minRotationDiff)
				rotating = true;
		}
	}

	protected struct HandTarget {
		public Vector3 target;
		public float urgency;

		public HandTarget(Vector3 target, float urgency) {
			this.target = target;
			this.urgency = urgency;
		}
	}
}
