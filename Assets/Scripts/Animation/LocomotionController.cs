using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class LocomotionController : MonoBehaviour {

	public SpiderLegController[] legGroup1;
	public SpiderLegController[] legGroup2;

	public float stanceWidth = 2;
	public float stanceHeight = 3;
	public float strideLength = 1;
	public float stepHeight = 0.5f;
	public float maxStepHeight = .8f;

	public float minMoveSpeed = 0.01f;
	public float minimumTimePerSwitch = 0.02f;

	public AudioSource footstep;
	public EffectSpec footPlant;

	public bool debug = false;

	private Dictionary<SpiderLegController, LegInfo> legInfo = new Dictionary<SpiderLegController, LegInfo>();
	private Vector3 lastPos;
	private SpiderLegController[] plantedGroup = null;
	private SpiderLegController[] steppingGroup = null;
	private float lastSwitch = 0;

	void Update() {
		//if (velocity.sqrMagnitude < minMoveSpeed) {
			// plant all legs
		//} else {
			// select stepping group
		if (plantedGroup == null) {
			plantedGroup = legGroup1;
			steppingGroup = legGroup2;
		}

		float stepPercent = Mathf.Min(1, calculateStepPercent(plantedGroup));
		//print(stepPercent);
		updateSteppingGroup(steppingGroup, stepPercent);
		float time = Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
		bool isSwitching = stepPercent > 0.99f && lastSwitch <= time - minimumTimePerSwitch;
		if (isSwitching) {
			SpiderLegController[] temp = plantedGroup;
			plantedGroup = steppingGroup;
			steppingGroup = temp;
			lastSwitch = time;
		}
		updateLegs(plantedGroup, true);
		updateLegs(steppingGroup, false);
	}

	protected float calculateStepPercent(SpiderLegController[] plantedGroup) {
		float offsetMagnitude = 0;
		foreach (SpiderLegController leg in plantedGroup) {
			LegInfo info = getLegInfo(leg);

			// maybe the following should be added back in?
			//if (info.planted)
			//	continue;

			Vector3 normalizedVelocity = info.getVelocity().normalized;
			Vector3 offset = info.foot - leg.getDefaultPos();
			offsetMagnitude = Mathf.Max(offsetMagnitude, Vector3.Dot(offset /strideLength, -normalizedVelocity) +1);
		}
		return offsetMagnitude / 2;
	}

	protected void updateSteppingGroup(SpiderLegController[] steppingGroup, float stepPercent) {
		foreach (SpiderLegController leg in steppingGroup) {
			LegInfo info = getLegInfo(leg);
			Vector3 stepOffset = leg.getDefaultPos() +info.getVelocity().normalized * strideLength;


			stepOffset.y += calculateAltitudeAdjustment(stepOffset, leg);
			Vector3 target = Vector3.Lerp(info.getLastPlanted(), stepOffset, stepPercent);

			target.y += Mathf.Min((1 - Mathf.Abs(stepPercent - 0.5f) * 2) * stepHeight, maxStepHeight);
			Vector3 diff = (target - info.foot);
			float deltaTime = Application.isPlaying ? Time.deltaTime : 0.03f;
			float maxDistance = Mathf.Max(10f, diff.magnitude * 20) * deltaTime;
            info.foot += diff.normalized * Mathf.Min(maxDistance, diff.magnitude);

#if UNITY_EDITOR
			if(debug) {
				drawPoint(info.foot, Color.blue, leg + "three");
			}
#endif
		}
	}
#if UNITY_EDITOR
	private Dictionary<string, GameObject> footPos = new Dictionary<string,GameObject>();
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

	protected float calculateAltitudeAdjustment(Vector3 stepOffset, SpiderLegController leg) {
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


	protected void updateLegs(SpiderLegController[] group, bool planted) {
		foreach(SpiderLegController leg in group) {
			LegInfo info = getLegInfo(leg);
			leg.setPosition(getLegInfo(leg).foot);
			info.setPlanted(planted);
			info.setLastDefault();
		}
	}

	protected LegInfo getLegInfo(SpiderLegController leg) {
		LegInfo info;
		if (!legInfo.TryGetValue(leg, out info)) {
			info = new LegInfo(this, leg);
			legInfo.Add(leg, info);
		}
		return info;
	}



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
			if (!planted && this.planted)
				setLastPlanted();
			if (planted && !this.planted) {
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
			return leg.getDefaultPos() -lastDefault;
		}

		public void setLastDefault() {
			lastDefault = leg.getDefaultPos();
		}

		protected void setLastPlanted() {
			lastPlanted = leg.transform.InverseTransformPoint(foot);
		}
	}
}
