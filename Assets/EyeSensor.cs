using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EyeSensor : MonoBehaviour {

    public float sightDistance = 30.0f;
    public float fieldOfViewAngle = 170f;           // Number of degrees, centered on forward, for the enemy sight.

    public bool canSee(Transform obj) {

        Vector3 objPos = obj.position;
        bool result = false;
        if (Vector3.Distance(objPos, transform.position) < sightDistance) {
            RaycastHit hit;
            Vector3 dir = objPos - transform.position;
            dir.Normalize();
            float angle = Vector3.Angle(dir, transform.forward);
            //			print (getController().gameObject.name);
            //			print (angle);

            if (angle < fieldOfViewAngle * 0.5f) {
                Physics.Raycast(transform.position, dir, out hit, sightDistance);
                if (hit.transform == obj) {//&& Vector3.Dot (transform.forward.normalized, (objPos - transform.position).normalized) > 0) {
                    result = true;
                    //if (getController().debug)
                }
            }
        }
        return result;
    }



}
