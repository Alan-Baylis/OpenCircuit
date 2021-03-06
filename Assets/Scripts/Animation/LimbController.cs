using UnityEngine;
using System;

[ExecuteInEditMode]
public abstract class LimbController : MonoBehaviour {

	public Vector3 defaultPos;

	public virtual void init() { }

	public virtual Vector3 getDefaultPos() {
		return transform.TransformPoint(defaultPos);
	}

	public abstract bool setPosition(Vector3 worldPos);

	public abstract Vector3 deducePosition();

	// TODO: add unit tests
	protected static double circleMidPointDistance(Vector2 p1, Vector2 p2, double r1, double r2) {
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

	// TODO: add unit tests
	protected static Vector3 rotate(Vector3 vector, Vector3 angle) {
		return Quaternion.Euler(angle) *vector;
	}

	// TODO: add unit tests
	protected static float getVectorAngle(float x, float y) {
		return Mathf.Atan2(y, x) *Mathf.Rad2Deg;
	}

	// TODO: add unit tests
	protected static float flipAngle(float angle) {
		return (angle +360) %360 -180;
	}

	// TODO: add unit tests
	protected static float clampAngle(float startAngle, float min, float max) {
		if (startAngle < max) {
			startAngle = max - (max - startAngle) % 360;
		} else if (startAngle > min) {
			startAngle = min + (startAngle - min) % 360;
		}
		return Mathf.Clamp(startAngle, min, max);
	}

	// TODO: add unit tests
	protected static bool clampAngle(ref float angle, float min, float max) {
		float clampedAngle = clampAngle(angle, min, max);
		if (clampedAngle % 360 != angle % 360) {
			angle = clampedAngle;
			return true;
		}
		return false;
	}

	[Serializable]
	public struct SegmentConfig {
		public Vector3 offset, tip;
		public float deadzone, minRotation, maxRotation;
	}

	protected class Segment {
		public SegmentConfig config;
		public Transform trans;
		public Segment parent;

		public Segment(SegmentConfig config, Transform trans, Segment parent=null) {
			this.config = config;
			this.trans = trans;
			this.parent = parent;
		}

		public Vector3 getTip() {
			return trans.TransformPoint(config.tip);
		}

		public float getLength() {
			return (config.tip - config.offset).magnitude;
		}

		public void reposition() {
			trans.localPosition = trans.localRotation * -config.offset;
			if (parent != null)
				trans.localPosition += parent.config.tip;
		}
	}

}
