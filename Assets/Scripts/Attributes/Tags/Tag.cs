using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public class Tag : InspectorListElement {

	public static Tag constructDefault() {
		return new Tag((TagEnum)Enum.GetValues(typeof(TagEnum)).GetValue(0), 0, null);
	}

	public static Tag constructTag(TagEnum type) {
		switch (type) {
			case TagEnum.PatrolRoute:
				return new PatrolTag(0, null);
			default:
				return new Tag(type, 0, null);
		}
	}

	public static readonly TagEnum[] tagEnums;

	public TagEnum type;
	public float severity;

	private string labelId;

	[System.NonSerialized]
	protected Dictionary<System.Type, HashSet<RobotController>> executors = new Dictionary<System.Type, HashSet<RobotController>>();

	[System.NonSerialized]
    private LabelHandle labelHandle;

	public Tag(TagEnum type, float severity, LabelHandle handle) {
		this.type = type;
		this.severity = severity;
		labelHandle = handle;
	}

    public void setLabelHandle(LabelHandle label) {
        labelHandle = label;
    }

    public LabelHandle getLabelHandle() {
        return labelHandle;
    }

	public void addExecution(RobotController executor, System.Type endeavourType) {
		getExecutors(endeavourType).Add(executor);
	}

	public void removeExecution(RobotController executor, System.Type endeavourType) {
		getExecutors(endeavourType).Remove(executor);
	}

	public int getConcurrentExecutions(RobotController executor, System.Type endeavourType) {
		HashSet<RobotController> endeavourExecutors = getExecutors(endeavourType);
		if (endeavourExecutors.Contains(executor)) {
			return endeavourExecutors.Count - 1;
		}
		return endeavourExecutors.Count;
	}

	private HashSet<RobotController> getExecutors(System.Type endeavourType) {
		if (executors == null) {
			executors = new Dictionary<Type, HashSet<RobotController>>();
		}
		if (!executors.ContainsKey(endeavourType)) {
			executors[endeavourType] = new HashSet<RobotController>();
		}
		return executors[endeavourType];
	}

#if UNITY_EDITOR
	InspectorListElement InspectorListElement.doListElementGUI(GameObject parent) {

		TagEnum newType = (TagEnum)UnityEditor.EditorGUILayout.Popup((int)type, Enum.GetNames(typeof(TagEnum)));
		if (newType != type) {
			return constructTag(newType);
		}
		doGUI(parent);
		return this;
	}

    public virtual void drawGizmo(Label label) {

    }

	public virtual void doGUI(GameObject parent) {
		severity = UnityEditor.EditorGUILayout.FloatField("Severity: ", severity);
	}
#endif
}