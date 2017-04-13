using UnityEngine;

[System.Serializable]
public class PatrolTag : AbstractRouteTag {


    public PatrolTag (float severity, LabelHandle labelHandle) : base(TagEnum.PatrolRoute, severity, labelHandle) {

    }

#if UNITY_EDITOR
    public override void drawGizmo(Label label) {
        //Color COLOR_ONE = Color.black;
        //Color COLOR_TWO = Color.green;
        Gizmos.color = Color.black;

        for (int i = 0; i < getPoints(label.gameObject).Count; ++i) {
            if (getPoints(label.gameObject)[i] == null)
                continue;
            int NUM_STRIPES = 8;
            Label current = getPoints(label.gameObject)[i];
            Label next = (i == getPoints(label.gameObject).Count - 1) ? getPoints(label.gameObject)[0] : getPoints(label.gameObject)[i + 1];
            if (next == null || current == null) {
                return;
            }
            float LENGTH = Vector3.Distance(current.transform.position, next.transform.position);
            Vector3 dir = next.transform.position - current.transform.position;
            dir.Normalize();
            Quaternion rotation = Quaternion.LookRotation(dir);
            for (int j = 0; j < NUM_STRIPES * LENGTH; j = j + 2) {
                //Gizmos.color = j % 2 == 0 ? COLOR_ONE : COLOR_TWO;
                Vector3 startPos = current.transform.position + (j * dir / NUM_STRIPES);
                Vector3 endPos = startPos + dir / NUM_STRIPES;
                if (Vector3.Distance(current.transform.position, endPos) > LENGTH) {
                    endPos = next.transform.position;
                }
                if (j % 8 == 0) {
                    UnityEditor.Handles.color = Color.white;
                    UnityEditor.Handles.ConeCap(0, (startPos + endPos) / 2, rotation, .15f);
                } else {
                    Gizmos.DrawLine(startPos, endPos);
                }
            }
        }
    }
#endif
}
