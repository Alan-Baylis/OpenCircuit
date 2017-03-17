using UnityEngine;

[ExecuteInEditMode]
public class ArmController : LimbController {

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
		return hand.getTip();
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
		upper.reposition();
		return true;

//		bool canReach = true;
//		Vector3 eulerAngles = upper.trans.localEulerAngles;
//		eulerAngles.y = 0;
//		upper.trans.localEulerAngles = eulerAngles;
//		upper.trans.localPosition = upper.offset;
//		Vector3 localPos = upper.trans.InverseTransformPoint(worldPos);
//		eulerAngles.y = (getVectorAngle(localPos.z, localPos.x) + 180 -upper.minRotation) % 360 - 180 +upper.minRotation;
//
//		// calculate circle intersection
//		double midPointDistance = circleMidPointDistance(new Vector2(upper.offset.x, upper.offset.z), new Vector2(localPos.x, localPos.z), upper.length, lower.length);
//		if (midPointDistance < upper.length) {
//			float angleOffset = (float)Math.Acos(midPointDistance / upper.length) * Mathf.Rad2Deg;
//			if (float.IsNaN(angleOffset) || angleOffset < 0) {
//				angleOffset = 180;
//			}
//			eulerAngles.y += angleOffset;
//		}
//
//		// apply angle limits
//		float clampedAngle = clampAngle(eulerAngles.y, upper.minRotation, upper.maxRotation);
//		if (clampedAngle % 360 != eulerAngles.y % 360) {
//			eulerAngles.y = clampedAngle;
//			canReach = false;
//		}
//		upper.trans.localPosition += rotate(-upper.offset, new Vector3(0, eulerAngles.y, 0));
//		upper.trans.localEulerAngles = eulerAngles;
//		return canReach;
	}

	private bool calculateLowerRotation(Vector3 worldPos) {
		bool canReach = true;
		Vector3 diff = lower.parent.trans.InverseTransformPoint(worldPos) -lower.parent.config.tip;
		float rotation = 0;
		if (new Vector2(diff.x, diff.y).sqrMagnitude > lower.config.deadzone)
			rotation = getVectorAngle(diff.x, diff.y) -
							 getVectorAngle(lower.config.tip.x -lower.config.offset.x,
								 lower.config.tip.y -lower.config.offset.y);

		float clampedAngle = clampAngle(rotation, lower.config.minRotation, lower.config.maxRotation);
		if (clampedAngle % 360 != rotation % 360) {
			rotation = clampedAngle;
			canReach = false;
		}

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
