using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public abstract class Operation: InspectorListElement {

	private static string[] typeNames = null;

    private static System.Type[] oTypes;

    public static System.Type[] types {
        get {
            if (oTypes == null) {
                System.Type[] ts = System.Reflection.Assembly.GetAssembly(typeof(Operation)).GetTypes();
                List<System.Type> pairedTypes = new List<System.Type>();
                System.Type targetType = typeof(Operation);
                foreach (System.Type t in ts) {
                    if (t.IsSubclassOf(targetType) && !t.IsAbstract)
                        pairedTypes.Add(t);
                }
                oTypes = pairedTypes.ToArray();
            }
            return oTypes;
        }
    }

	[System.NonSerialized]
	protected Label parent;

	public abstract System.Type[] getTriggers();

	public abstract void perform(GameObject instigator, Trigger trig);
	
	public virtual void doGUI() {}

	public void setParent(Label label) {
		this.parent = label;
	}

	public static Operation constructDefault() {
		return (Operation) types[0].GetConstructor(new System.Type[0]).Invoke(new object[0]);
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
	InspectorListElement InspectorListElement.doListElementGUI(GameObject parent) {
		int selectedType = System.Array.FindIndex(types, OP => OP == GetType());
		int newSelectedType = UnityEditor.EditorGUILayout.Popup(selectedType, getTypeNames());
		if (newSelectedType != selectedType) {
			return (Operation)Operation.types[newSelectedType].GetConstructor(new System.Type[0]).Invoke(new object[0]);
		}

		doGUI();
		return this;
	}
#endif
}