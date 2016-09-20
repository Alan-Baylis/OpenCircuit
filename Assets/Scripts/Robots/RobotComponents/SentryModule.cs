using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SentryModule : AbstractVisualSensor {

    public Rotatable rotatable;


    void Update() {

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

    private void scan() {
        if (Mathf.Abs(Vector3.Dot(rotatable.transform.forward, Vector3.up)) > .0001f) { //if we are not on the horizontal plane
            //rotate to the horizontal plane
            rotatable.transform.rotation = Quaternion.RotateTowards(rotatable.transform.rotation, Quaternion.LookRotation(new Vector3(rotatable.transform.forward.x, 0, rotatable.transform.forward.z).normalized), rotatable.rotationSpeed * Time.deltaTime);
        } else {
            //otherwise scan on you crazy diamond
            rotatable.transform.Rotate(Vector3.up, rotatable.rotationSpeed * Time.deltaTime);
        }
    }

    private void trackTarget(Vector3 pos) {

        rotatable.transform.rotation = Quaternion.RotateTowards(rotatable.transform.rotation, Quaternion.LookRotation(pos - rotatable.transform.position), rotatable.rotationSpeed * Time.deltaTime);
    }
}
