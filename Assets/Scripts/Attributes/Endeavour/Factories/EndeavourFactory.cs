using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[System.Serializable]
public abstract class EndeavourFactory : InspectorListElement {

	public List<Goal> goals = new List<Goal> ();
	private bool status = false;
	private int size = 0;

	[System.NonSerialized]
	protected Label parent;

	private static string[] typeNames = null;

	public static readonly System.Type[] types = new System.Type[] {
		typeof(Patrol),
		typeof(Pursue),
		typeof(Drop),
        typeof(Recharge),
		typeof(ScanAtStationFactory),
		typeof(Guard),
		typeof(Hunt)
	};

	public static readonly GoalEnum[] goalEnums;

	static EndeavourFactory() {
		Array values = Enum.GetValues(typeof(GoalEnum));
		List<GoalEnum> typeList = new List<GoalEnum>();
		foreach(object obj in values) {
			typeList.Add((GoalEnum)obj);
		}
		goalEnums = typeList.ToArray();
	}

	public void setParent(Label parent) {
		this.parent = parent;
	}

	public abstract Endeavour constructEndeavour (RobotController controller);

	public static EndeavourFactory constructDefault() {
		EndeavourFactory factory = (EndeavourFactory) types[0].GetConstructor(new System.Type[0]).Invoke(new object[0]);
		factory.goals = new List<Goal> ();
		return factory;
	}

	public virtual void drawGizmo() {
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(parent.transform.position, .2f);
	}
	
	private static string[] getTypeNames() {
		if (typeNames == null || typeNames.Length != types.Length) {
			typeNames = new string[types.Length];
			for(int i=0; i<typeNames.Length; ++i) {
				typeNames[i] = types[i].FullName;
			}
		}
		return typeNames;
	}

#if UNITY_EDITOR
	InspectorListElement InspectorListElement.doListElementGUI() {
		int selectedType = System.Array.FindIndex(types, OP => OP == GetType());
		int newSelectedType = UnityEditor.EditorGUILayout.Popup(selectedType, getTypeNames());
		if (newSelectedType != selectedType) {
			return (EndeavourFactory)EndeavourFactory.types[newSelectedType].GetConstructor(new System.Type[0]).Invoke(new object[0]);
		}

		doGUI();
		return this;
	}

	public virtual void doGUI() {
		status = UnityEditor.EditorGUILayout.Foldout(status, "Goals");

		if (status && goals != null) {
			size = UnityEditor.EditorGUILayout.IntField("Size:", goals.Count);
			UnityEditor.EditorGUILayout.Separator();

			foreach (Goal goal in goals) {
				//goal.name = EditorGUILayout.TextField("Name", goal.name);
				//int selectedType = (int) goal.type; // System.Array.FindIndex(goalEnums, OP => OP == goal.type);
				goal.type = (GoalEnum) UnityEditor.EditorGUILayout.Popup((int) goal.type, Enum.GetNames(typeof(GoalEnum)));
				goal.priority = UnityEditor.EditorGUILayout.FloatField("Priority", goal.priority);
				UnityEditor.EditorGUILayout.Separator();
			}
			if (size < goals.Count) {
				goals.RemoveRange(size, goals.Count - size);
			}
			while (size > goals.Count) {
				goals.Add(new Goal((GoalEnum)Enum.GetValues(typeof(GoalEnum)).GetValue(0), 0));
			}
		}
	}
#endif
}