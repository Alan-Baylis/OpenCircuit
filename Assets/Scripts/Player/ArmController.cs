using System;
using UnityEngine;

[ExecuteInEditMode]
public class ArmController : LimbController {

	public SegmentInfo shoulder;
	public SegmentInfo upper;
	public SegmentInfo lower;
	public SegmentInfo hand;

	private Transform arm;

	public void Awake () {
		init();
		setPosition(getDefaultPos());
	}

	public override void init() {
		arm = GetComponent<Transform>();
		shoulder.trans = arm.FindChild("Shoulder");
		upper.trans = shoulder.trans.FindChild("Upper Arm");
		lower.trans = upper.trans.FindChild("Lower Arm");
		hand.trans = lower.trans.FindChild("Hand");
		shoulder.setLength();
		upper.setLength();
		lower.setLength();
		hand.setLength();
	}

	public override bool setPosition(Vector3 worldPos) {
		bool canReach = true;

		// calculate hip rotation
		canReach &= calculatShoulderRotation(worldPos);

		// calculate upper leg rotation
		canReach &= calculateUpperRotation(worldPos);

		// calculate lower leg rotation
		canReach &= calculateLowerRotation(worldPos);

		return canReach;
	}

	public override Vector3 deducePosition() {
		return hand.trans.position;
	}

	private bool calculatShoulderRotation(Vector3 worldPos) {
		bool canReach = true;
		Vector3 eulerAngles = shoulder.trans.localEulerAngles;
		float rotation = eulerAngles.y;
		eulerAngles.y = 0;
		shoulder.trans.localEulerAngles = eulerAngles;
		Vector3 localPos = shoulder.trans.InverseTransformPoint(worldPos);
		if (new Vector2(localPos.x, localPos.y).sqrMagnitude > 0.02)
			rotation = getVectorAngle(-localPos.y, localPos.x) +90;
		eulerAngles.y = rotation;
		if (eulerAngles.y < shoulder.minRotation || eulerAngles.y > shoulder.maxRotation) {
			float newAngle = flipAngle(eulerAngles.y);
			if (newAngle < shoulder.minRotation || newAngle > shoulder.maxRotation) {
				eulerAngles.y = clampAngle(eulerAngles.y, shoulder.minRotation, shoulder.maxRotation);
				canReach = false;
			} else {
				eulerAngles.y = newAngle;
			}
		}
		shoulder.trans.localEulerAngles = eulerAngles;
		return canReach;
	}

	private bool calculateUpperRotation(Vector3 worldPos) {
		bool canReach = true;
		Vector3 eulerAngles = upper.trans.localEulerAngles;
		eulerAngles.y = 0;
		upper.trans.localEulerAngles = eulerAngles;
		upper.trans.localPosition = upper.offset;
		Vector3 localPos = upper.trans.InverseTransformPoint(worldPos);
		eulerAngles.y = (getVectorAngle(localPos.z, localPos.x) + 180 -upper.minRotation) % 360 - 180 +upper.minRotation;

		// calculate circle intersection
		double midPointDistance = circleMidPointDistance(new Vector2(upper.offset.x, upper.offset.z), new Vector2(localPos.x, localPos.z), upper.length, lower.length);
		if (midPointDistance < upper.length) {
			float angleOffset = (float)Math.Acos(midPointDistance / upper.length) * Mathf.Rad2Deg;
			if (float.IsNaN(angleOffset) || angleOffset < 0) {
				angleOffset = 180;
			}
			eulerAngles.y += angleOffset;
		}

		// apply angle limits
		float clampedAngle = clampAngle(eulerAngles.y, upper.minRotation, upper.maxRotation);
		if (clampedAngle % 360 != eulerAngles.y % 360) {
			eulerAngles.y = clampedAngle;
			canReach = false;
		}
		upper.trans.localPosition += rotate(-upper.offset, new Vector3(0, eulerAngles.y, 0));
		upper.trans.localEulerAngles = eulerAngles;
		return canReach;
	}

	private bool calculateLowerRotation(Vector3 worldPos) {
		bool canReach = true;
		Vector3 eulerAngles = lower.trans.localEulerAngles;
		eulerAngles.y = 0;
		lower.trans.localEulerAngles = eulerAngles;
		lower.trans.localPosition = lower.offset;
		Vector3 localPos = lower.trans.InverseTransformPoint(worldPos);
		eulerAngles.y = (getVectorAngle(localPos.z, localPos.x) + 180 -lower.minRotation) % 360 -180 +lower.minRotation;

		eulerAngles.y += 180;
		float clampedAngle = clampAngle(eulerAngles.y, lower.minRotation, lower.maxRotation);
		if (clampedAngle % 360 != eulerAngles.y % 360) {
			eulerAngles.y = clampedAngle;
			canReach = false;
		}
		lower.trans.localPosition += rotate(-lower.offset, new Vector3(0, eulerAngles.y, 0));
		lower.trans.localEulerAngles = eulerAngles;
		return canReach;
	}
}
