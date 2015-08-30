﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


[System.Serializable]
public abstract class Operation: InspectorListElement {

	private static string[] typeNames = null;
	public static readonly System.Type[] types = new System.Type[] {
		typeof(OpenOperation),
		typeof(DamageOperation),
		typeof(EMPOp),
	};

	[System.NonSerialized]
	protected Label parent;

	public virtual System.Type[] getTriggers() {
		return new System.Type[0];
	}

	public virtual void perform(GameObject instigator, Trigger trig) {
	}
	
	public virtual void doGUI() {}

	public void setParent(Label label) {
		this.parent = label;
	}

	InspectorListElement InspectorListElement.doListElementGUI() {
		int selectedType = System.Array.FindIndex(types, OP => OP == GetType());
		int newSelectedType = EditorGUILayout.Popup(selectedType, getTypeNames());
		if (newSelectedType != selectedType) {
			return (Operation) Operation.types[newSelectedType].GetConstructor(new System.Type[0]).Invoke(new object[0]);
		}

		doGUI();
		return this;
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
}