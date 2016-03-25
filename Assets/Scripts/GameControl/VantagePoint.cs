using UnityEngine;
using System.Collections;

public class VantagePoint : MonoBehaviour {

	public GameObject vantagePoint;
	public GameObject[] lookAtPoints;
	public float pauseTime = .5f;
	public float lookSpeed = .8f;


	private bool triggered = false;
	private float timeSincePause = 0f;
	private int currentLookAt = 0;
	private bool isPaused = false;
	

	private Player victim;
	void Update() {
		if(triggered && lookAtPoints != null && lookAtPoints.Length > 0) {
			if(!victim.mover.isAutoMoving() && !victim.looker.isAutoMode()) {
				if(!isPaused) {
					isPaused = true;
				} else if(timeSincePause > pauseTime) {
					timeSincePause = 0;
					isPaused = false;
					if(lookAtPoints != null && currentLookAt < lookAtPoints.Length) {
						GameObject nextPoint = lookAtPoints[currentLookAt];
						++currentLookAt;

						while(nextPoint == null && currentLookAt < lookAtPoints.Length) {
							nextPoint = lookAtPoints[currentLookAt];
							++currentLookAt;
						}
						if(nextPoint == null) {
							return;
						}
						victim.looker.lookAtPoint(nextPoint.transform.position, lookSpeed);
					} else {
						victim.controls.enablePlayerControls();

					}
				}
			}
			if(isPaused) {
				timeSincePause += Time.deltaTime;
			}
		}
	}

	public void OnTriggerEnter(Collider other) {
		if(!triggered) {
			victim = other.gameObject.GetComponent<Player>();
			if(victim == null)
				return;
			victim.controls.disablePlayerControls();

			//print(vantagePoint.transform.position);
			if(vantagePoint != null) {
				victim.mover.moveToPoint(vantagePoint.transform.position);

				victim.looker.lookAtPoint(new Vector3(vantagePoint.transform.position.x, transform.position.y, vantagePoint.transform.position.z), lookSpeed);
			}
			triggered = true;
		}
		
	}
}
