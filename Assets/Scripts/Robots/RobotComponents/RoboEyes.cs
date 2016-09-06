using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Scripts/Robot/Robo Eyes")]
public class RoboEyes : AbstractVisualSensor {

	private LaserProjector scanner;

	// Use this for initialization
	[ServerCallback]
	public override void Start () {
        base.Start();
		scanner = GetComponent<LaserProjector>();
	}

	public GameObject lookAt(Vector3 position) {
		if(powerSource.hasPower(Time.deltaTime)) {
			Vector3 direction = position - transform.position;
			RaycastHit hitInfo;
			if(Physics.Raycast(transform.position, direction, out hitInfo, direction.magnitude)) {
#if UNITY_EDITOR
				if (getController().debug)
					drawLine(transform.position, hitInfo.point, Color.green);
#endif
				return hitInfo.collider.gameObject;
			}
		}

#if UNITY_EDITOR
		if (getController().debug)
			drawLine(transform.position, position, Color.red);
#endif
		return null;
	}

	public bool hasScanner() {
		return scanner != null;
	}

	public LaserProjector getScanner() {
		return scanner;
	}

	protected override bool canSee (Transform obj) {
		Vector3 objPos = obj.position;
		bool result = false;
		if (Vector3.Distance (objPos, transform.position) < sightDistance) {
			RaycastHit hit;
			Vector3 dir = objPos - transform.position;
			dir.Normalize();
			float angle = Vector3.Angle(dir, transform.forward);
//			print (getController().gameObject.name);
//			print (angle);
			if(angle < fieldOfViewAngle * 0.5f) {
				Physics.Raycast (transform.position, dir, out hit, sightDistance);
				if (hit.transform == obj ) {//&& Vector3.Dot (transform.forward.normalized, (objPos - transform.position).normalized) > 0) {
					result = true;
#if UNITY_EDITOR
					if (getController().debug)
						drawLine(transform.position, hit.point, Color.green);
#endif
				} else {
					//print("looking for: " + obj.gameObject.name);
					//print("blocked by: " + hit.collider.gameObject.name);
#if UNITY_EDITOR
					if (getController().debug)
						drawLine(transform.position, hit.point, Color.red);
#endif
					//print("lost: " + obj.gameObject.name + "obscured by: " + hit.transform.gameObject.name);
				}
			}
		}
		return result;
	}
}
