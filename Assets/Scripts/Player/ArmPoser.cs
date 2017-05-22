using UnityEngine;

[ExecuteInEditMode]
public class ArmPoser : LimbController {

	public SegmentConfig shoulderConfig;
	public SegmentConfig upperArmConfig;
	public SegmentConfig lowerArmConfig;
	public SegmentConfig handConfig;

	private Transform arm;
	private Segment shoulder;
	private Segment upper;
	private Segment lower;
	private Segment hand;

	public void Awake () {
		init();
		setPosition(getDefaultPos());
	}

	public override void init() {
		arm = GetComponent<Transform>();
		shoulder = new Segment(shoulderConfig, arm.FindChild("Shoulder"));
		upper = new Segment(upperArmConfig, shoulder.trans.FindChild("Upper Arm"), shoulder);
		lower = new Segment(lowerArmConfig, upper.trans.FindChild("Lower Arm"), upper);
		hand = new Segment(handConfig, lower.trans.FindChild("Hand"), lower);
	}

	public override bool setPosition(Vector3 worldPos) {
		bool canReach = true;

		// calculate hip rotation
		canReach &= calculatShoulderRotation(worldPos);

		// calculate upper leg rotation
		canReach &= calculateUpperRotation(worldPos);

		// calculate lower leg rotation
		canReach &= calculateLowerRotation(worldPos);

		// calculate hand rotation
		canReach &= calculateHandRotation(worldPos);

		return canReach;
	}

	public override Vector3 deducePosition() {
		return lower.getTip();
	}

	public Vector3 deduceHandPosition() {
		return hand.getTip();
	}

	public bool setHandPositionAndRotation(Vector3 position, Quaternion rotation) {
		hand.trans.localRotation = rotation;
		hand.reposition();
		return true;
	}

	private bool calculatShoulderRotation(Vector3 worldPos) {
		bool canReach = true;
		Vector3 eulerAngles = shoulder.trans.localEulerAngles;
		float rotation = eulerAngles.y;
		eulerAngles.y = 0;
		shoulder.trans.localEulerAngles = eulerAngles;
		Vector3 localPos = shoulder.trans.InverseTransformPoint(worldPos);
		if (new Vector2(localPos.x, localPos.z).sqrMagnitude > shoulder.config.deadzone *shoulder.config.deadzone)
			rotation = -getVectorAngle(localPos.x, localPos.z);

		if (rotation < shoulder.config.minRotation || rotation > shoulder.config.maxRotation) {
			float newAngle = flipAngle(rotation);
			if (newAngle < shoulder.config.minRotation || newAngle > shoulder.config.maxRotation) {
				rotation = clampAngle(rotation, shoulder.config.minRotation, shoulder.config.maxRotation);
				canReach = false;
			} else {
				rotation = newAngle;
			}
		}
		eulerAngles.y = rotation;
		shoulder.trans.localEulerAngles = eulerAngles;
		shoulder.reposition();
		return canReach;
	}

	private bool calculateUpperRotation(Vector3 worldPos) {
		bool canReach = true;
		Vector3 diff = upper.parent.trans.InverseTransformPoint(worldPos) -upper.parent.config.tip;
		float rotation = 0;
		rotation = getVectorAngle(diff.x, diff.y) -	getVectorAngle(
			lower.config.tip.x -lower.config.offset.x,
			lower.config.tip.y -lower.config.offset.y);

		// calculate circle intersection
		double midPointDistance = circleMidPointDistance(new Vector2(upper.config.offset.x, upper.config.offset.y),
			new Vector2(diff.x, diff.y), upper.getLength(), lower.getLength());
		if (midPointDistance < upper.getLength()) {
			float angleOffset = (float)System.Math.Acos(midPointDistance / upper.getLength()) * Mathf.Rad2Deg;
			if (float.IsNaN(angleOffset) || angleOffset < 0) {
				angleOffset = 180;
			}
			rotation += angleOffset;
		}

		// apply angle limits
		canReach = !clampAngle(ref rotation, upper.config.minRotation, upper.config.maxRotation);

		Vector3 eulerAngles = upper.trans.localEulerAngles;
		eulerAngles.z = rotation;
		upper.trans.localEulerAngles = eulerAngles;
	    upper.reposition();
		return canReach;
	}

	private bool calculateLowerRotation(Vector3 worldPos) {
		bool canReach = true;
		Vector3 diff = lower.parent.trans.InverseTransformPoint(worldPos) -lower.parent.config.tip;
		float rotation = 0;
		if (new Vector2(diff.x, diff.y).sqrMagnitude > lower.config.deadzone)
			rotation = getVectorAngle(diff.x, diff.y) -
							 getVectorAngle(lower.config.tip.x -lower.config.offset.x,
								 lower.config.tip.y -lower.config.offset.y);

		canReach = !clampAngle(ref rotation, lower.config.minRotation, lower.config.maxRotation);

		Vector3 eulerAngles = lower.trans.localEulerAngles;
		eulerAngles.z = rotation;
		lower.trans.localEulerAngles = eulerAngles;
		lower.reposition();
		return canReach;
	}

	private bool calculateHandRotation(Vector3 worldPos) {
		hand.reposition();
		return true;
	}
}
