using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegPoser : LimbController {
	
	public SegmentConfig hipConfig;
	public SegmentConfig upperLegConfig;
	public SegmentConfig lowerLegConfig;

	private Transform leg;
	private Segment hip;
	private Segment upper;
	private Segment lower;

	public void Awake () {
		init();
		setPosition(getDefaultPos());
	}

	public override void init() {
		leg = GetComponent<Transform>();
		hip = new Segment(hipConfig, leg.FindChild("Hip"));
		upper = new Segment(upperLegConfig, hip.trans.FindChild("Upper Leg"), hip);
		lower = new Segment(lowerLegConfig, upper.trans.FindChild("Lower Leg"), upper);
	}

	public override bool setPosition(Vector3 worldPos) {
		bool canReach = true;

		// calculate hip rotation
		canReach &= calculatHipRotation(worldPos);

		// calculate upper leg rotation
		canReach &= calculateUpperRotation(worldPos);

		// calculate lower leg rotation
		canReach &= calculateLowerRotation(worldPos);

		return canReach;
	}

	public override Vector3 deducePosition() {
		return lower.getTip();
	}

	private bool calculatHipRotation(Vector3 worldPos) {
		bool canReach = true;
		Vector3 eulerAngles = hip.trans.localEulerAngles;
		float rotation = eulerAngles.y;
		eulerAngles.y = 0;
		hip.trans.localEulerAngles = eulerAngles;
		Vector3 localPos = hip.trans.InverseTransformPoint(worldPos);
		if (new Vector2(localPos.x, localPos.z).sqrMagnitude > hip.config.deadzone *hip.config.deadzone)
			rotation = -getVectorAngle(localPos.x, localPos.z);

		if (rotation < hip.config.minRotation || rotation > hip.config.maxRotation) {
			float newAngle = flipAngle(rotation);
			if (newAngle < hip.config.minRotation || newAngle > hip.config.maxRotation) {
				rotation = clampAngle(rotation, hip.config.minRotation, hip.config.maxRotation);
				canReach = false;
			} else {
				rotation = newAngle;
			}
		}
		eulerAngles.y = rotation;
		hip.trans.localEulerAngles = eulerAngles;
		hip.reposition();
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
			rotation -= angleOffset;
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
}
