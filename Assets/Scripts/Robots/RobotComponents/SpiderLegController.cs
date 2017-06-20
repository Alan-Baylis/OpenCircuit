using UnityEngine;

[ExecuteInEditMode]
public class SpiderLegController : LimbController {

	public float hipMinRotation = -90;
	public float hipMaxRotation = 90;

	public Vector3 upperOffset;
	public float upperAngleOffset = 7.26f;
	public float upperMinRotation = -90;
	public float upperMaxRotation = 90;

	public Vector3 lowerOffset;
	public float lowerAngleOffset = 1.8f;
	public float lowerLegLength;
	public float lowerMinRotation = -90;
	public float lowerMaxRotation = 90;

	private Transform leg;
	private Transform hip;
	private Transform upperLeg;
	private Transform lowerLeg;

	private float upperLegLength;
	
	private float upperMinRotationPlusAngleOffset;
	private float lowerMinRotationPlusAngleOffset;
	private Vector2 upperOffsetXZ;
	private Vector3 negativeUpperOffset;
	private Vector3 negativeLowerOffset;
	
	public void Awake() {
		leg = GetComponent<Transform>();
		hip = leg.FindChild("Hip");
		upperLeg = hip.FindChild("Upper Leg");
		lowerLeg = upperLeg.FindChild("Lower Leg");
		upperLegLength = (upperOffset - lowerOffset).magnitude;
		setPosition(getDefaultPos());
		upperMinRotationPlusAngleOffset = upperMinRotation + upperAngleOffset;
		lowerMinRotationPlusAngleOffset = lowerMinRotation + lowerAngleOffset;
		upperOffsetXZ = new Vector2(upperOffset.x, upperOffset.z);
		negativeUpperOffset = -upperOffset;
		negativeLowerOffset = -lowerOffset;
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
		return lowerLeg.TransformVector(
			       rotate(new Vector3(0, 0, -lowerLegLength), new Vector3(0, -lowerAngleOffset, 0))
			       +lowerOffset) +lowerLeg.position;
	}

	private bool calculatHipRotation(Vector3 worldPos) {
		bool canReach = true;
		Vector3 eulerAngles = hip.localEulerAngles;
		float rotation = eulerAngles.y;
		eulerAngles.y = 0;
		hip.localEulerAngles = eulerAngles;
		Vector3 localPos = hip.InverseTransformPoint(worldPos);
		if (new Vector2(localPos.x, localPos.y).sqrMagnitude > 0.02)
			rotation = getVectorAngle(-localPos.y, localPos.x) +90;
		eulerAngles.y = rotation;
		if (eulerAngles.y < hipMinRotation || eulerAngles.y > hipMaxRotation) {
			float newAngle = flipAngle(eulerAngles.y);
			if (newAngle < hipMinRotation || newAngle > hipMaxRotation) {
				eulerAngles.y = clampAngle(eulerAngles.y, hipMinRotation, hipMaxRotation);
				canReach = false;
			} else {
				eulerAngles.y = newAngle;
			}
		}
		hip.localEulerAngles = eulerAngles;
		return canReach;
	}

	private bool calculateUpperRotation(Vector3 worldPos) {
		bool canReach = true;
		Vector3 eulerAngles = upperLeg.localEulerAngles;
		eulerAngles.y = 0;
		upperLeg.localEulerAngles = eulerAngles;
		upperLeg.localPosition = upperOffset;
		Vector3 localPos = upperLeg.InverseTransformPoint(worldPos);
		eulerAngles.y = (getVectorAngle(localPos.z, localPos.x) + 180 -upperMinRotation) % 360 - 180 +upperMinRotationPlusAngleOffset;

		// calculate circle intersection
		double midPointDistance = circleMidPointDistance(upperOffsetXZ, new Vector2(localPos.x, localPos.z), upperLegLength, lowerLegLength);
		if (midPointDistance < upperLegLength) {
			float angleOffset = (float)System.Math.Acos(midPointDistance / upperLegLength) * Mathf.Rad2Deg;
			if (float.IsNaN(angleOffset) || angleOffset < 0) {
				angleOffset = 180;
			}
			eulerAngles.y += angleOffset;
		}

		// apply angle limits
		float clampedAngle = clampAngle(eulerAngles.y, upperMinRotation, upperMaxRotation);
		if (clampedAngle % 360 != eulerAngles.y % 360) {
			eulerAngles.y = clampedAngle;
			canReach = false;
		}
		upperLeg.localPosition += rotate(negativeUpperOffset, new Vector3(0, eulerAngles.y, 0));
		upperLeg.localEulerAngles = eulerAngles;
		return canReach;
	}

	private bool calculateLowerRotation(Vector3 worldPos) {
		bool canReach = true;
		Vector3 eulerAngles = lowerLeg.localEulerAngles;
		eulerAngles.y = 0;
		lowerLeg.localEulerAngles = eulerAngles;
		lowerLeg.localPosition = lowerOffset;
		Vector3 localPos = lowerLeg.InverseTransformPoint(worldPos);
		eulerAngles.y = (getVectorAngle(localPos.z, localPos.x) + 180 -lowerMinRotation) % 360 -180 +lowerMinRotationPlusAngleOffset;

		eulerAngles.y += 180;
		float clampedAngle = clampAngle(eulerAngles.y, lowerMinRotation, lowerMaxRotation);
		if (clampedAngle % 360 != eulerAngles.y %360) {
			eulerAngles.y = clampedAngle;
			canReach = false;
		}
		lowerLeg.localPosition += rotate(negativeLowerOffset, new Vector3(0, eulerAngles.y, 0));
		lowerLeg.localEulerAngles = eulerAngles;
		return canReach;
	}
}
