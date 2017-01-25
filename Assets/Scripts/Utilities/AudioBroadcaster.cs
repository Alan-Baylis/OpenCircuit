using UnityEngine;

public static class AudioBroadcaster {

	public static void broadcast(LabelHandle source, float volume) {
	 foreach (AudioSensor sensor in AudioSensor.sensors) {
		 //TODO find a way to gracefully cull this list
		 if (sensor == null) {
			 continue;
		 }
		 //Debug.Log("adjusted sensor range: " + sensor.getRange() * volume);
		 //Debug.Log("sound distanct: " + Vector3.Distance(sensor.transform.position, TargetPos));
		 if (Vector3.Distance(sensor.transform.position, source.getPosition()) < (sensor.getRange() * volume)) {
			 sensor.processAudioEvent(source);
		 }
	 }
 }

}
