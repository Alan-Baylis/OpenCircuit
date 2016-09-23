using UnityEngine;
using System.Collections;

public class Rotatable : MonoBehaviour {

    public float rotationSpeed = 1;

    public enum Axis {
        X_AXIS, Y_AXIS, Z_AXIS
    }

    public Axis axis;

}
