using UnityEngine;

[System.Serializable]
public class GuardTag : Tag {

    public const float NUM_STRIPES = 8f;
    public const float LENGTH = 1f;


    public static Color COLOR_ONE = Color.black;
    public static Color COLOR_TWO = Color.yellow;

    public GuardTag(float severity, LabelHandle handle) : base(TagEnum.GuardPoint, severity, handle) {

    }

#if UNITY_EDITOR
    public override void drawGizmo(Label label) {
        float sphereSize = .2f;
        for (int i = 0; i < NUM_STRIPES; i++) {
            Gizmos.color = i % 2 == 0 ? COLOR_ONE : COLOR_TWO;
            Vector3 startPos = label.transform.position + (label.transform.forward * (sphereSize - .02f)) + ((i * (LENGTH / NUM_STRIPES)) * label.transform.forward);
            Vector3 endPos = startPos + LENGTH / NUM_STRIPES * label.transform.forward;
            Gizmos.DrawLine(startPos, endPos);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(label.transform.position, sphereSize);
    }
#endif
}
