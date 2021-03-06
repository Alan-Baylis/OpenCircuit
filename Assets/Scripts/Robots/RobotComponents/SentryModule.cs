﻿using UnityEngine;
using UnityEngine.Networking;

public class SentryModule : AbstractVisualSensor {

    public Rotatable rotatable;
	public AudioSource alertSound;

	private bool alerted = false;

	[ServerCallback]
    void FixedUpdate() {

        if (getSightingCount() > 0) { //no point if there are no targets in view
            Vector3 averageLocation = Vector3.zero;
            foreach (SensoryInfo target in targetMap.Values) {
                if (target.getSightings() > 0) {
                    averageLocation += target.getPosition();
                }
            }
            averageLocation /= getSightingCount();
#if UNITY_EDITOR
            if (isComponentAttached() && getController().debug) {
                //TODO: Surround this with a debug check...
                drawLine(eye.transform.position, averageLocation, Color.blue);
            }
#endif
            trackTarget(averageLocation);
        } else {
            //scan;
            scan();
        }
    }

	[ServerCallback]
	public void Update() {
		// play alert sound
		bool alert = false;
		if (isComponentAttached() && getSightingCount() > 0) {
			foreach (Label label in targetMap.Keys) {
				if (targetMap[label].getSightings() > 0
					&& label.hasTag(TagEnum.Player)
					&& !label.hasTag(TagEnum.Frozen)) {
					alert = true;
					break;
				}
			}
		}
		if (alert != alerted) {
			RpcSetAlert(alert);
			alerted = alert;
		}
	}

	[Server]
    private void scan() {
        if (Mathf.Abs(Vector3.Dot(rotatable.transform.forward, Vector3.up)) > .0001f) { //if we are not on the horizontal plane
            //rotate to the horizontal plane
            rotatable.transform.rotation = Quaternion.RotateTowards(rotatable.transform.rotation, Quaternion.LookRotation(new Vector3(rotatable.transform.forward.x, 0, rotatable.transform.forward.z).normalized), rotatable.rotationSpeed * Time.deltaTime);
        } else {
            //otherwise scan on you crazy diamond
            rotatable.transform.Rotate(Vector3.up, rotatable.rotationSpeed * Time.deltaTime);
        }
    }

	[Server]
    private void trackTarget(Vector3 pos) {
        rotatable.transform.rotation = Quaternion.RotateTowards(rotatable.transform.rotation, Quaternion.LookRotation(pos - rotatable.transform.position), rotatable.rotationSpeed * Time.deltaTime);
    }
	
	protected override void dismantleEffect() {
		stopAlertSound();
		alerted = false;
		base.dismantleEffect();
	}

	[ClientRpc]
	private void RpcSetAlert(bool alert) {
		if (alertSound == null)
			return;
		if (alert) {
			if (!alertSound.isPlaying)
				alertSound.Play();
		} else {
			stopAlertSound();
		}
	}

	private void stopAlertSound() {
		if (alertSound.isPlaying) {
			alertSound.Stop();
		}
	}
}
