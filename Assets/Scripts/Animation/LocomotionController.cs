﻿using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class LocomotionController : MonoBehaviour {

	public LimbController[] legGroup1;
	public LimbController[] legGroup2;

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

	private Dictionary<LimbController, LegInfo> legInfo = new Dictionary<LimbController, LegInfo>();
	private Vector3 lastPos;
	private LimbController[] plantedGroup;
	private LimbController[] steppingGroup;
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

	public void Update() {
		//double startTime = Time.realtimeSinceStartup;
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
				if (stoppingPercent == 1 && getMaxDisplacement(steppingGroup) == 0) {
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
		//double endTime = Time.realtimeSinceStartup;
//		controller.getExecutionTimer().addTime(endTime-startTime);
	}

	public float getMaxSpeed() {
		float maxSpeed = 0;
		foreach(LimbController leg in plantedGroup) {
			maxSpeed = Mathf.Max(maxSpeed, getLegInfo(leg).getVelocity().magnitude);
		}
		return maxSpeed;
	}

	public void swapLegGroups() {
		LimbController[] temp = plantedGroup;
		plantedGroup = steppingGroup;
		steppingGroup = temp;
		lastStepPercent = 0;
	}

	protected float getMaxDisplacement(LimbController[] group) {
		float sqrDisplacement = 0;
		foreach(LimbController leg in group) {
			LegInfo info = getLegInfo(leg);
			
			Vector3 offset = info.foot - leg.getDefaultPos();
			offset.y = 0;
			sqrDisplacement = Mathf.Max(sqrDisplacement, offset.sqrMagnitude);
		}
		float displacement = Mathf.Sqrt(sqrDisplacement);
		return displacement;
	}

	protected bool isGroupPlanted(LimbController[] group) {
		foreach(LimbController leg in group) {
			if (!getLegInfo(leg).planted)
				return false;
		}
		return true;
	}

	protected float calculateStepPercent(LimbController[] plantedGroup, float strideLength) {
		float offsetMagnitude = 0;
		foreach (LimbController leg in plantedGroup) {
			LegInfo info = getLegInfo(leg);
			Vector3 normalizedVelocity = info.getVelocity().normalized;
			Vector3 offset = info.foot - leg.getDefaultPos();
			offsetMagnitude = Mathf.Max(offsetMagnitude, Vector3.Dot(offset /strideLength, -normalizedVelocity) +1);
		}
		return offsetMagnitude / 2;
	}

	protected bool updateSteppingGroup(LimbController[] steppingGroup, float stepPercent) {
		bool airborne = true;
		float halfStepHeight = stepHeight * 0.5f;
		float quadrupleNormalMoveSpeed = 4 * normalMoveSpeed;
		float doubleStepHeight = 2 * stepHeight;
		foreach (LimbController leg in steppingGroup) {
			LegInfo info = getLegInfo(leg);
			Vector3 velocity = info.getVelocity();
			velocity.y = 0;
			float strideLength = getStrideLength(velocity.magnitude);
			Vector3 stepOffset = leg.getDefaultPos() + velocity.normalized * strideLength;

			float altitudeAdjustment = calculateAltitudeAdjustment(stepOffset, leg);
			if (altitudeAdjustment == float.MinValue) {
				stepOffset.y -= halfStepHeight;
			} else {
				airborne = false;
				stepOffset.y += altitudeAdjustment;
				stepOffset.y += Mathf.Min(stepHeight - Mathf.Abs(stepPercent - 0.5f) *doubleStepHeight, maxStepHeight);
			}
			Vector3 target = Vector3.Lerp(info.getLastPlanted(), stepOffset, stepPercent);
			Vector3 diff = (target - info.foot);
			float maxDistance = Mathf.Max(quadrupleNormalMoveSpeed, diff.magnitude * 20) * deltaTime;
			info.foot += diff.normalized * Mathf.Min(maxDistance, diff.magnitude);
#if UNITY_EDITOR
			drawPoint(info.foot, Color.blue, leg + "three");
#endif
		}
		return airborne;
	}

	protected float calculateAltitudeAdjustment(Vector3 stepOffset, LimbController leg) {
		Vector3 maxStepPos = stepOffset + new Vector3(0, maxStepHeight, 0);
		RaycastHit hitInfo;
		float yOffset = 0f;
		if (Physics.Raycast(new Ray(maxStepPos, new Vector3(0, -1, 0)), out hitInfo, stepHeightRange)) {
			yOffset = hitInfo.point.y - stepOffset.y;

#if UNITY_EDITOR
			drawPoint(hitInfo.point, Color.green, leg + "one");
			drawPoint(stepOffset, Color.red, leg + "two");
#endif
		} else {
			return float.MinValue;
		}
		return yOffset;
	}


	protected void updateLegs(LimbController[] group, bool planted) {
		foreach(LimbController leg in group) {
			LegInfo info = getLegInfo(leg);
			bool canReach = leg.setPosition(info.foot);
			if (!canReach)
				info.foot = leg.deducePosition();
			info.setPlanted(planted);
			info.setLastDefault();
#if UNITY_EDITOR
			drawPoint(leg.getDefaultPos (), Color.cyan, "default pos - " + leg);
#endif
		}
	}

	protected float getStrideLength(float speed) {
		return Mathf.Min(maxStrideLength, speed / normalMoveSpeed *normalStrideLength);
	}

	protected LegInfo getLegInfo(LimbController leg) {
		LegInfo info;
		if (!legInfo.TryGetValue(leg, out info)) {
			info = new LegInfo(this, leg);
			legInfo.Add(leg, info);
		}
		return info;
	}

#if UNITY_EDITOR
	private void drawPoint(Vector3 point, Color color, string id) {
		if (debug)
			debugPoints[id] = new DebugPoint() {point=point, color=color};
	}
#endif


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
		public bool planted;
		public Vector3 foot = Vector3.zero;
		public Vector3 lastDefault = Vector3.zero;
		public Vector3 lastPlanted = Vector3.zero;
		public LocomotionController chassis;
		public LimbController leg;
		public ParticleSystem particleSystem;
		public AudioSource soundEmitter;

		public LegInfo(LocomotionController chassis, LimbController leg) {
			this.chassis = chassis;
			this.leg = leg;
			foot = leg.getDefaultPos();
			setLastDefault();
			setLastPlanted();
			particleSystem = leg.GetComponentInChildren<ParticleSystem>();
			soundEmitter = leg.GetComponentInChildren<AudioSource>();

		}

		public void setPlanted(bool planted) {
			if (!planted && this.planted) {
				setLastPlanted();
			} else if (planted && !this.planted) {
				if (particleSystem != null) {
					particleSystem.Play();
				} else {
					Debug.LogWarning("Missing footstep particle system on '" + chassis.transform.root.gameObject.name + "'!");
				}
				if (soundEmitter != null) {
					soundEmitter.Play();
				} else {
					Debug.LogWarning("Missing footstep audio source on '" + chassis.transform.root.gameObject.name + "'!");
				}
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
