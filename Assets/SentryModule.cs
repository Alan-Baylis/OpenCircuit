using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SentryModule : AbstractVisualSensor {

    public Rotatable rotatable;


    void Update() {
        Vector3 averageLocation = Vector3.zero;

        int count = 0;
        foreach (SensoryInfo target in targetMap.Values) {
            if (target.getSightings() > 0) {
                averageLocation += target.getPosition();
                count++;
            }
        }

        if (count > 0) {
            averageLocation /= (float)count;
        } else {
            averageLocation = eye.transform.position + transform.forward;
        }
        drawLine(eye.transform.position, averageLocation, Color.blue);
        trackTarget(averageLocation);
    }

    private void trackTarget(Vector3 pos) {

        rotatable.transform.rotation = Quaternion.LookRotation(pos - rotatable.transform.position);
    }
}
