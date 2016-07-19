using UnityEngine;
using System.Collections;

public class AudioEvent : RobotMessage {

	//TODO velocity must not be null when we improve the investigate system!
	public AudioEvent(Vector3 source, LabelHandle handle, Vector3 sourcePos)
		: base(RobotMessage.MessageType.TARGET_SIGHTED, "sound heard", handle, sourcePos, null) {
    }

    public void broadcast(float volume) {
        foreach (AudioSensor sensor in AudioSensor.sensors) {
            //TODO find a way to gracefully cull this list
            if (sensor == null) {
                continue;
            }
			//Debug.Log("adjusted sensor range: " + sensor.getRange() * volume);
			//Debug.Log("sound distanct: " + Vector3.Distance(sensor.transform.position, TargetPos));
            if (Vector3.Distance(sensor.transform.position, TargetPos) < (sensor.getRange() * volume)) {
                sensor.processAudioEvent(this);
            }
        }
    }
}
