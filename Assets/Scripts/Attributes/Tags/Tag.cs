using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class  Tag : InspectorListElement {

	public static Tag constructDefault() {
		return new Tag((TagEnum)Enum.GetValues(typeof(TagEnum)).GetValue(0), 0);
	}

	public static readonly TagEnum[] tagEnums;

	public TagEnum type;
	public float severity;

    [System.NonSerialized]
    private Label label;

	public Tag(TagEnum type, float severity) {
		this.type = type;
		this.severity = severity;
	}

    public void setLabel(Label label) {
        this.label = label;
    }

    public Label getLabel() {
        return label;
    }

#if UNITY_EDITOR
	InspectorListElement InspectorListElement.doListElementGUI() {
		type = (TagEnum)UnityEditor.EditorGUILayout.Popup((int)type, Enum.GetNames(typeof(TagEnum)));

		doGUI();
		return this;
	}

    public virtual void drawGizmo() {

    }

	public virtual void doGUI() {
		severity = UnityEditor.EditorGUILayout.FloatField("Severity: ", severity);
	}
#endif
}