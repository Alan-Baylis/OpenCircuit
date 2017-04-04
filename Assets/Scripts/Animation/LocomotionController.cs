using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class LocomotionController : MonoBehaviour {

	public SpiderLegController[] legGroup1;
	public SpiderLegController[] legGroup2;

	public float stanceWidth = 2;
	public float stanceHeight = 3;
	public float normalStrideLength = 1;
	public float maxStrideLength = 1.25f;
	public float stepHeight = 0.5f;
	public float maxStepHeight = .8f;
	public float stepHeightRange = 1f;

	public float minMoveSpeed = 0.01f;
	public float normalMoveSpeed = 2;
	public float minimumTimePerSwitch = 0.02f;
	public float minStepRate = 1;

	public EffectSpec footPlant;

	public bool debug;

	private Dictionary<SpiderLegController, LegInfo> legInfo = new Dictionary<SpiderLegController, LegInfo>();
	private Vector3 lastPos;
	private SpiderLegController[] plantedGroup;
	private SpiderLegController[] steppingGroup;
	private float lastSwitch;
	private bool stopped = true;
	private bool moving;
	private float stoppingPercent;
	private float lastStepPercent;
	private bool airborne;

	public bool isAirborne { get { return airborne; } }

	// these handle running in editor
	protected float time {
		get {
			return Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
		}
	}
	protected float deltaTime {
		get {
			return Application.isPlaying ? Time.deltaTime : 0.03f;
		}
	}

	void Update() {
		if (plantedGroup == null) {
			plantedGroup = legGroup1;
			steppingGroup = legGroup2;
		}

		float maxSpeed = getMaxSpeed();
		if (maxSpeed < minMoveSpeed) {
			moving = false;
			// plant feet
			if (!stopped) {
				updateSteppingGroup(steppingGroup, stoppingPercent);
				if (getMaxDisplacement(steppingGroup) == 0 && stoppingPercent == 1) {
					if (getMaxDisplacement(plantedGroup) == 0) {
						stopped = true;
					} else {
						swapLegGroups();
						stoppingPercent = 0;
					}
				}
				stoppingPercent = Mathf.Min(1, stoppingPercent +minStepRate *2 *Time.deltaTime);
			}
		} else if (airborne) {
			updateSteppingGroup(plantedGroup, 0.2f);
			airborne = updateSteppingGroup(steppingGroup, 0.8f);
		} else {
			moving = true;
			stopped = false;
			// step feet
			float maxStrideLength = Mathf.Max(normalStrideLength, getStrideLength(maxSpeed));
			float minStepChange = minStepRate * Time.deltaTime;
			float stepPercent = Mathf.Min(1, Mathf.Max(lastStepPercent + minStepChange,
				calculateStepPercent(plantedGroup, maxStrideLength)));
			lastStepPercent = stepPercent;
			airborne = updateSteppingGroup(steppingGroup, stepPercent);
			bool isSwitching = stepPercent > 0.99f && lastSwitch <= time - minimumTimePerSwitch;
			if (isSwitching && !airborne) {
				swapLegGroups();
				lastSwitch = time;
			}
			stoppingPercent = stepPercent;
		}
		if (!airborne) {
			updateLegs(plantedGroup, true);
			updateLegs(steppingGroup, stopped);
		}
	}

	public float getMaxSpeed() {
		float maxSpeed = 0;
		foreach(SpiderLegController leg in plantedGroup) {
			maxSpeed = Mathf.Max(maxSpeed, getLegInfo(leg).getVelocity().magnitude);
		}
		return maxSpeed;
	}

	public void swapLegGroups() {
		SpiderLegController[] temp = plantedGroup;
		plantedGroup = steppingGroup;
		steppingGroup = temp;
		lastStepPercent = 0;
	}

	protected float getMaxDisplacement(SpiderLegController[] group) {
		float sqrDisplacement = 0;
		foreach(SpiderLegController leg in group) {
			LegInfo info = getLegInfo(leg);
			
			Vector3 offset = info.foot - leg.getDefaultPos();
			offset.y = 0;
			sqrDisplacement = Mathf.Max(sqrDisplacement, offset.sqrMagnitude);
		}
		float displacement = Mathf.Sqrt(sqrDisplacement);
		return displacement;
    }

	protected bool isGroupPlanted(SpiderLegController[] group) {
		foreach(SpiderLegController leg in group) {
			if (!getLegInfo(leg).planted)
				return false;
		}
		return true;
	}

	protected float calculateStepPercent(SpiderLegController[] plantedGroup, float strideLength) {
		float offsetMagnitude = 0;
		foreach (SpiderLegController leg in plantedGroup) {
			LegInfo info = getLegInfo(leg);
			Vector3 normalizedVelocity = info.getVelocity().normalized;
			Vector3 offset = info.foot - leg.getDefaultPos();
			offsetMagnitude = Mathf.Max(offsetMagnitude, Vector3.Dot(offset /strideLength, -normalizedVelocity) +1);
		}
		return offsetMagnitude / 2;
	}

	protected bool updateSteppingGroup(SpiderLegController[] steppingGroup, float stepPercent) {
		bool airborne = true;
		foreach (SpiderLegController leg in steppingGroup) {
			LegInfo info = getLegInfo(leg);
			Vector3 velocity = info.getVelocity();
			velocity.y = 0;
			float strideLength = getStrideLength(velocity.magnitude);
			Vector3 stepOffset = leg.getDefaultPos() + velocity.normalized * strideLength;

			float altitudeAdjustment = calculateAltitudeAdjustment(stepOffset, leg);
			if (altitudeAdjustment == float.MinValue) {
				stepOffset.y -= stepHeight *0.5f;
			} else {
				airborne = false;
				stepOffset.y += altitudeAdjustment;
				stepOffset.y += Mathf.Min((1 - Mathf.Abs(stepPercent - 0.5f) * 2) * stepHeight, maxStepHeight);
			}
			Vector3 target = Vector3.Lerp(info.getLastPlanted(), stepOffset, stepPercent);
			Vector3 diff = (target - info.foot);
			float maxDistance = Mathf.Max(normalMoveSpeed *4, diff.magnitude * 20) * deltaTime;
			info.foot += diff.normalized * Mathf.Min(maxDistance, diff.magnitude);

			drawPoint(info.foot, Color.blue, leg + "three");
		}
		return airborne;
	}

	protected float calculateAltitudeAdjustment(Vector3 stepOffset, SpiderLegController leg) {
		Vector3 maxStepPos = stepOffset + new Vector3(0, maxStepHeight, 0);
		RaycastHit hitInfo;
		float yOffset = 0f;
		if (Physics.Raycast(new Ray(maxStepPos, new Vector3(0, -1, 0)), out hitInfo, stepHeightRange)) {
			yOffset = hitInfo.point.y - stepOffset.y;

			drawPoint(hitInfo.point, Color.green, leg + "one");
			drawPoint(stepOffset, Color.red, leg + "two");
		} else {
			return float.MinValue;
		}
		return yOffset;
	}


	protected void updateLegs(SpiderLegController[] group, bool planted) {
		foreach(SpiderLegController leg in group) {
			LegInfo info = getLegInfo(leg);
			bool canReach = leg.setPosition(info.foot);
			if (!canReach)
				info.foot = leg.deducePosition();
			info.setPlanted(planted);
			info.setLastDefault();

			drawPoint(leg.getDefaultPos (), Color.cyan, "default pos - " + leg);
		}
	}

	protected float getStrideLength(float speed) {
		return Mathf.Min(maxStrideLength, speed / normalMoveSpeed *normalStrideLength);
	}

	protected LegInfo getLegInfo(SpiderLegController leg) {
		LegInfo info;
		if (!legInfo.TryGetValue(leg, out info)) {
			info = new LegInfo(this, leg);
			legInfo.Add(leg, info);
		}
		return info;
	}

	private void drawPoint(Vector3 point, Color color, string id) {
#if UNITY_EDITOR
		if (debug)
			debugPoints[id] = new DebugPoint() {point=point, color=color};
#endif
	}

#if UNITY_EDITOR
	private Dictionary<string, DebugPoint> debugPoints = new Dictionary<string, DebugPoint>();

	public void OnDrawGizmos() {
		if (!debug)
			return;

		// draw moving status
		if (moving) {
			Gizmos.color = Color.green;
		} else if (stopped) {
			Gizmos.color = Color.red;
		} else {
			Gizmos.color = new Color(1, 0.5f, 0);
		}
		Gizmos.DrawSphere(transform.position, 0.2f);

		// draw points
		foreach(DebugPoint point in debugPoints.Values) {
			Gizmos.color = point.color;
			Gizmos.DrawCube(point.point, Vector3.one * 0.2f);
		}
	}

	private struct DebugPoint {
		public Vector3 point;
		public Color color;
	}
#endif



	protected class LegInfo {
		public bool planted = false;
		public Vector3 foot = Vector3.zero;
		public Vector3 lastDefault = Vector3.zero;
		public Vector3 lastPlanted = Vector3.zero;
		public LocomotionController chassis;
		public SpiderLegController leg;

		public LegInfo(LocomotionController chassis, SpiderLegController leg) {
			this.chassis = chassis;
			this.leg = leg;
			foot = leg.getDefaultPos();
			setLastDefault();
			setLastPlanted();
		}

		public void setPlanted(bool planted) {
			if (!planted && this.planted) {
				setLastPlanted();
			} else if (planted && !this.planted) {
				chassis.footPlant.spawn(foot, Vector3.up);
			}
			this.planted = planted;
		}

		public Vector3 getLastPlanted() {
			return leg.transform.TransformPoint(lastPlanted);
		}

		public Vector3 getVelocity() {
			return (leg.getDefaultPos() -lastDefault) /chassis.deltaTime;
		}

		public void setLastDefault() {
			lastDefault = leg.getDefaultPos();
		}

		protected void setLastPlanted() {
			lastPlanted = leg.transform.InverseTransformPoint(foot);
		}
	}
}
