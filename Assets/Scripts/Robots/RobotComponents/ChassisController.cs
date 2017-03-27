using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ChassisController : MonoBehaviour {

	public LegController[] legGroup1;
	public LegController[] legGroup2;

	public float stanceWidth = 2;
	public float stanceHeight = 3;
	public float normalStrideLength = 1;
	public float maxStrideLength = 1.25f;
	public float stepHeight = 0.5f;
	public float maxStepHeight = .8f;

	public float minMoveSpeed = 0.01f;
	public float normalMoveSpeed = 2;
	public float minimumTimePerSwitch = 0.02f;

	public AudioSource footstep;
	public EffectSpec footPlant;

	public bool debug = false;

	private Dictionary<LegController, LegInfo> legInfo = new Dictionary<LegController, LegInfo>();
	private LegController[] plantedGroup = null;
	private LegController[] steppingGroup = null;
	private float lastSwitch = 0;
	private bool stopped = true;

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
		
		float stepPercent = 0;
		float maxSpeed = getMaxSpeed();
		if (maxSpeed < minMoveSpeed) {
			// plant feet
			if (!stopped) {
				updateSteppingGroup(steppingGroup, 1, 0);
				if (getMaxDisplacement(steppingGroup) == 0) {
					if (getMaxDisplacement(plantedGroup) == 0) {
						stopped = true;
					} else {
						swapLegGroups();
					}
				}
			}
		} else {
			stopped = false;
			// step feet
			float strideLength = Mathf.Min(maxStrideLength, maxSpeed / normalMoveSpeed *normalStrideLength);
			stepPercent = Mathf.Min(1, calculateStepPercent(plantedGroup, strideLength));
			updateSteppingGroup(steppingGroup, stepPercent, strideLength);
			bool isSwitching = stepPercent > 0.99f && lastSwitch <= time - minimumTimePerSwitch;
			if (isSwitching) {
				swapLegGroups();
				lastSwitch = time;
			}
		}

		updateLegs(plantedGroup, true);
		updateLegs(steppingGroup, stopped);
	}

	public float getMaxSpeed() {
		float maxSpeed = 0;
		foreach(LegController leg in plantedGroup) {
			maxSpeed = Mathf.Max(maxSpeed, getLegInfo(leg).getVelocity().magnitude);
		}
		return maxSpeed;
	}

	public void swapLegGroups() {
		LegController[] temp = plantedGroup;
		plantedGroup = steppingGroup;
		steppingGroup = temp;
	}

	protected float getMaxDisplacement(LegController[] group) {
		float displacement = 0;
		foreach (LegController leg in group) {
			LegInfo info = getLegInfo(leg);
			
			Vector3 offset = info.foot - leg.getDefaultPos();
			displacement = Mathf.Max(displacement, offset.magnitude);
		}
		print("displacement: " + displacement);
		return displacement;
    }

	protected float calculateStepPercent(LegController[] plantedGroup, float strideLength) {
		float offsetMagnitude = 0;
		foreach (LegController leg in plantedGroup) {
			LegInfo info = getLegInfo(leg);

			Vector3 normalizedVelocity = info.getVelocity().normalized;
			Vector3 offset = info.foot - leg.getDefaultPos();
			offsetMagnitude = Mathf.Max(offsetMagnitude, Vector3.Dot(offset /strideLength, -normalizedVelocity) +1);
		}
		return offsetMagnitude / 2;
	}

	protected void updateSteppingGroup(LegController[] steppingGroup, float stepPercent, float strideLength) {
		foreach (LegController leg in steppingGroup) {
			LegInfo info = getLegInfo(leg);
			Vector3 stepOffset = leg.getDefaultPos() +info.getVelocity().normalized * strideLength;
			//print("step offset: " + stepOffset);


			stepOffset.y += calculateAltitudeAdjustment(stepOffset, leg);
			Vector3 target = Vector3.Lerp(info.getLastPlanted(), stepOffset, stepPercent);

			target.y += Mathf.Min((1 - Mathf.Abs(stepPercent - 0.5f) * 2) * stepHeight, maxStepHeight);
			Vector3 diff = (target - info.foot);
			float maxDistance = Mathf.Max(normalMoveSpeed *4, diff.magnitude * 20) * deltaTime;
			print("diff: " + diff + "   max dis: " + maxDistance);
            info.foot += diff.normalized * Mathf.Min(maxDistance, diff.magnitude);

#if UNITY_EDITOR
			if(debug) {
				drawPoint(info.foot, Color.blue, leg + "three");
			}
#endif
		}
	}

	protected float calculateAltitudeAdjustment(Vector3 stepOffset, LegController leg) {
		Vector3 maxStepPos = stepOffset + new Vector3(0, maxStepHeight, 0);
		RaycastHit hitInfo = new RaycastHit();
		float yOffset = 0f;
		if(Physics.Raycast(new Ray(maxStepPos, new Vector3(0, -1, 0)), out hitInfo, maxStepHeight * 10)) {
#if UNITY_EDITOR
			if(debug) {
				drawPoint(hitInfo.point, Color.green, leg + "one");
				drawPoint(stepOffset, Color.red, leg + "two");
			}
#endif

			yOffset = hitInfo.point.y - stepOffset.y;
		}
		return yOffset;
	}


	protected void updateLegs(LegController[] group, bool planted) {
		foreach(LegController leg in group) {
			LegInfo info = getLegInfo(leg);
			leg.setPosition(getLegInfo(leg).foot);
			info.setPlanted(planted);
			info.setLastDefault();
			drawPoint(leg.getDefaultPos(), Color.cyan, "default pos - " + leg);
		}
	}

	protected LegInfo getLegInfo(LegController leg) {
		LegInfo info;
		if (!legInfo.TryGetValue(leg, out info)) {
			info = new LegInfo(this, leg);
			legInfo.Add(leg, info);
		}
		return info;
	}

#if UNITY_EDITOR
	private Dictionary<string, GameObject> footPos = new Dictionary<string, GameObject>();
	private void drawPoint(Vector3 point, Color color, string id) {
		if (footPos.ContainsKey(id))
			Destroy(footPos[id]);
		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.transform.position = point;
		cube.GetComponent<MeshRenderer>().material.color = color;
		Destroy(cube.GetComponent<BoxCollider>());
		cube.transform.localScale = new Vector3(.2f, .2f, .2f);
		footPos[id] = cube;
	}
#endif



	protected class LegInfo {
		public bool planted = false;
		public Vector3 foot = Vector3.zero;
		public Vector3 lastDefault = Vector3.zero;
		public Vector3 lastPlanted = Vector3.zero;
		public ChassisController chassis;
		public LegController leg;

		public LegInfo(ChassisController chassis, LegController leg) {
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
                if (chassis.footstep != null) {
					chassis.footstep.pitch = Random.Range(1f, 1.5f);
					chassis.footstep.Play();
				}
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
