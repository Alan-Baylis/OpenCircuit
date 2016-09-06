using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SentryModule : AbstractVisualSensor {

    public EyeSensor eye;
    public Rotatable rotatable;


    void Update() {
        clearLines();
        Vector3 averageLocation = Vector3.zero;
        foreach (SensoryInfo target in targetMap.Values) {
            drawLine(eye.transform.position, target.getPosition(), Color.green);
            averageLocation += target.getPosition();
        }
        averageLocation /= targetMap.Values.Count;
        trackTarget(averageLocation);
    }

    private void trackTarget(Vector3 pos) {
        rotatable.transform.rotation = Quaternion.LookRotation(pos - rotatable.transform.position);
    }

    protected override bool canSee(Transform transform) {
        return eye.canSee(transform);
    }
}
