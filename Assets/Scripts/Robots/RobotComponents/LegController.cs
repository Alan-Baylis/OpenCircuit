using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class LegController : MonoBehaviour {

	public Vector3 defaultPos = new Vector3(2, -3, 2);

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
	
	public void Awake () {
		leg = GetComponent<Transform>();
		hip = leg.FindChild("Hip");
		upperLeg = hip.FindChild("Upper Leg");
		lowerLeg = upperLeg.FindChild("Lower Leg");
		upperLegLength = (upperOffset - lowerOffset).magnitude;
		setPosition(getDefaultPos());
	}

	public Vector3 getDefaultPos() {
		return leg.TransformPoint(defaultPos);
	}

	//void Update() {
	//	if (target == null)
	//		return;
	//	setPosition(target.position);
	//	setPosition(getDefaultPos());
	//}

	public bool setPosition(Vector3 worldPos) {
		bool canReach = true;

		// calculate hip rotation
		canReach &= calculatHipRotation(worldPos);

		// calculate upper leg rotation
		canReach &= calculateUpperRotation(worldPos);

		// calculate lower leg rotation
		canReach &= calculateLowerRotation(worldPos);

		return canReach;
	}

	public Vector3 deducePosition() {
		return lowerLeg.TransformVector(rotate(new Vector3(0, 0, -lowerLegLength), new Vector3(0, -lowerAngleOffset, 0)) +lowerOffset) +lowerLeg.position;
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
		eulerAngles.y = (getVectorAngle(localPos.z, localPos.x) + 180 -upperMinRotation) % 360 - 180 +upperMinRotation +upperAngleOffset;

		// calculate circle intersection
		double midPointDistance = circleMidPointDistance(new Vector2(upperOffset.x, upperOffset.z), new Vector2(localPos.x, localPos.z), upperLegLength, lowerLegLength);
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
		upperLeg.localPosition += rotate(-upperOffset, new Vector3(0, eulerAngles.y, 0));
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
		eulerAngles.y = (getVectorAngle(localPos.z, localPos.x) + 180 -lowerMinRotation) % 360 -180 +lowerMinRotation +lowerAngleOffset;

		eulerAngles.y += 180;
		float clampedAngle = clampAngle(eulerAngles.y, lowerMinRotation, lowerMaxRotation);
		if (clampedAngle % 360 != eulerAngles.y %360) {
			eulerAngles.y = clampedAngle;
			canReach = false;
		}
		lowerLeg.localPosition += rotate(-lowerOffset, new Vector3(0, eulerAngles.y, 0));
		lowerLeg.localEulerAngles = eulerAngles;
		return canReach;
	}

	// TODO: add unit test
	private double circleMidPointDistance(Vector2 p1, Vector2 p2, double r1, double r2) {
		// ax + by + c = 0 is the equation for the line that passes through the circle intersection points
		double a = 2 * (p1.x - p2.x);
		double b = 2 * (p1.y - p2.y);
		double c = (r1 * r1 - r2 * r2) - (p1.x *p1.x -p2.x *p2.x) - (p1.y * p1.y - p2.y * p2.y);

		//return System.Math.Abs(a * p1.x + b * p1.y + c) /System.Math.Sqrt(a *a + b *b);

		Vector2 point = new Vector2(
			(float)(b *(b *p1.x - a *p1.y) - a *c),
			(float)(a *(a *p1.y - b *p1.x) - b *c)
			) /(float)(a *a + b * b);

		double sign = Vector2.Dot(point -p1, p2 -p1);
		double distance = (point - p1).magnitude;
		return sign > 0 ? distance : -distance;
	}

	// TODO: add unit test
	private Vector3 rotate(Vector3 vector, Vector3 angle) {
		return Quaternion.Euler(angle) *vector;
	}

	// TODO: add unit test
	private float getVectorAngle(float x, float y) {
		return Mathf.Atan2(y, x) *Mathf.Rad2Deg;
    }

	// TODO: add unit test
	private float flipAngle(float angle) {
		return (angle +360) %360 -180;
	}

	// TODO: add unit test
	private float clampAngle(float startAngle, float min, float max) {
		if (startAngle < max) {
			startAngle = max - ((max - startAngle) % 360);
		} else if (startAngle > min) {
			startAngle = min + ((startAngle - min) % 360);
		}
		return Mathf.Clamp(startAngle, min, max);
	}
}
