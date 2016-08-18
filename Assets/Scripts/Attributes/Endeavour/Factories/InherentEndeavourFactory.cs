using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public abstract class InherentEndeavourFactory : EndeavourFactory, InspectorListElement {

	private bool status = false;
	private int size = 0;

	private static string[] typeNames = null;

	public static new readonly System.Type[] types = new System.Type[] {
		typeof(Investigate),
	};

	private static string[] getTypeNames() {
		if(typeNames == null || typeNames.Length != types.Length) {
			typeNames = new string[types.Length];
			for(int i = 0; i < typeNames.Length; ++i) {
				typeNames[i] = types[i].FullName;
			}
		}
		return typeNames;
	}

	public static new InherentEndeavourFactory constructDefault() {
		InherentEndeavourFactory factory = (InherentEndeavourFactory)types[0].GetConstructor(new System.Type[0]).Invoke(new object[0]);
		factory.goals = new List<Goal>();
		return factory;
	}

	public abstract bool isApplicable(LabelHandle labelHandle);

	public abstract Endeavour constructEndeavour(RobotController controller, LabelHandle target);

	public override Endeavour constructEndeavour(RobotController controller) {
		return null;
	}

#if UNITY_EDITOR
	InspectorListElement InspectorListElement.doListElementGUI() {
		int selectedType = System.Array.FindIndex(types, OP => OP == GetType());
		int newSelectedType = UnityEditor.EditorGUILayout.Popup(selectedType, getTypeNames());
		if(newSelectedType != selectedType) {
			return (InherentEndeavourFactory)InherentEndeavourFactory.types[newSelectedType].GetConstructor(new System.Type[0]).Invoke(new object[0]);
		}

		doGUI();
		return this;
	}

	public override void doGUI() {
		base.doGUI();
	}
#endif
}
