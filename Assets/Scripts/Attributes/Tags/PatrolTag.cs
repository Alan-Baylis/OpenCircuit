using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class PatrolTag : Tag {

    [System.NonSerialized]
    public List<Label> points = null;
    [System.NonSerialized]
    private List<LabelHandle> pointHandles;
    private string[] pointsPaths = new string[0];

    private bool status = false;
    private int size = 0;

    public PatrolTag (float severity) : base(TagEnum.PatrolRoute, severity) {

    }

    public List<Label> getPoints() {
        if (points == null || points.Count < getPointsPaths().Length) {
            points = new List<Label>();
            foreach (string id in getPointsPaths()) {
                if (id == null)
                    points.Add(null);
                else
                    points.Add(ObjectReferenceManager.get().fetchReference<Label>(id));
            }
        }
        return points;
    }

    private string[] getPointsPaths() {
        if (pointsPaths == null) {
            pointsPaths = new string[0];
        }
        return pointsPaths;
    }

    public List<LabelHandle> getPointHandles() {
        if (pointHandles == null) {
            pointHandles = new List<LabelHandle>();
            foreach (Label label in getPoints()) {
                pointHandles.Add(label.labelHandle);
            }
        }
        return pointHandles;
    }

#if UNITY_EDITOR
    public override void doGUI() {
        base.doGUI();
        status = UnityEditor.EditorGUILayout.Foldout(status, "Points");

        if (status) {
            size = UnityEditor.EditorGUILayout.IntField("Size:", getPoints().Count);
            if (size < getPoints().Count) {
                getPoints().RemoveRange(size, getPoints().Count - size);
                string[] temp = new string[size];
                Array.Copy(pointsPaths, 0, temp, 0, size);
                pointsPaths = temp;
            } else if (size > getPoints().Count) {
                while (size > getPoints().Count) {
                    getPoints().Add(null);
                }
                string[] temp = new string[size];
                if (pointsPaths.Length != 0) {
                    Array.Copy(pointsPaths, 0, temp, 0, pointsPaths.Length - 1);
                }
                for (int i = pointsPaths.Length; i < size; i++) {
                    temp[i] = null;
                }
                pointsPaths = temp;
            }

            for (int i = 0; i < getPoints().Count; i++) {
                getPoints()[i] = ((Label)UnityEditor.EditorGUILayout.ObjectField(getPoints()[i], typeof(Label), true));
                if (getPoints()[i] != null) {
                    ObjectReferenceManager.get().deleteReference(getLabel(), getPointsPaths()[i]);
                    getPointsPaths()[i] = ObjectReferenceManager.get().addReference(getLabel(), getPoints()[i]);
                }
            }
        }
    }

    public override void drawGizmo() {
        //Color COLOR_ONE = Color.black;
        //Color COLOR_TWO = Color.green;
        Gizmos.color = Color.black;

        for (int i = 0; i < getPoints().Count; ++i) {
            if (getPoints()[i] == null)
                continue;
            int NUM_STRIPES = 8;
            Label current = getPoints()[i];
            Label next = (i == getPoints().Count - 1) ? getPoints()[0] : getPoints()[i + 1];
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
