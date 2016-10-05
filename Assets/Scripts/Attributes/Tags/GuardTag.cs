using UnityEngine;
using System.Collections;

public class GuardTag : Tag {

    public const float NUM_STRIPES = 8f;
    public const float LENGTH = 1f;


    public static Color COLOR_ONE = Color.black;
    public static Color COLOR_TWO = Color.yellow;

    public GuardTag(float severity) : base(TagEnum.GuardPoint, severity) {

    }

#if UNITY_EDITOR
    public override void drawGizmo() {
        float sphereSize = .2f;
        for (int i = 0; i < NUM_STRIPES; i++) {
            Gizmos.color = i % 2 == 0 ? COLOR_ONE : COLOR_TWO;
            Vector3 startPos = getLabel().transform.position + (getLabel().transform.forward * (sphereSize - .02f)) + ((i * (LENGTH / NUM_STRIPES)) * getLabel().transform.forward);
            Vector3 endPos = startPos + (((LENGTH / NUM_STRIPES)) * getLabel().transform.forward);
            Gizmos.DrawLine(startPos, endPos);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(getLabel().transform.position, sphereSize);
    }
#endif
}
