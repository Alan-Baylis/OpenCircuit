﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;


[CustomEditor(typeof(Label), true)]
public class LabelGUI : Editor {

	private bool tagsExpanded = true;
	private bool operationsExpanded = true;
	private bool endeavoursExpanded = true;
	private string[] operationTypeNames;
	
	public void OnEnable() {
		operationTypeNames = new string[Operation.types.Length];
		for(int i=0; i<operationTypeNames.Length; ++i) {
			operationTypeNames[i] = Operation.types[i].FullName;
		}
	}
	
	public override void OnInspectorGUI() {
		serializedObject.Update();
		Label label = (Label)target;
		EditorGUILayout.PropertyField(serializedObject.FindProperty("isVisible"));
		doTagList(label);
		doOperationList(label);
		doEndeavourList(label);
		serializedObject.ApplyModifiedProperties();

		
		// finally, apply the changes
		label.OnBeforeSerialize();
		EditorUtility.SetDirty(target);
		serializedObject.Update();	
		serializedObject.ApplyModifiedProperties();
	}

	public void doTagList(Label label) {

		tagsExpanded = EditorGUILayout.Foldout(tagsExpanded, "Tags");
		if(!tagsExpanded) {
			return;
		}

		for(int i = 0; i < label.tags.Length; ++i)
			if(label.tags[i] == null)
				label.tags[i] = Tag.constructDefault();
		doArrayGUI(ref label.tags);
		/*
		int newSize = UnityEditor.EditorGUILayout.IntField("Size:", label.tags.Length);
			if(newSize != label.tags.Length) {
				if(newSize < label.tags.Length) {
					//shrink tags array
					Tag[] temp = new Tag[newSize];
					Array.Copy(label.tags, 0, temp, 0, newSize);
					label.tags = temp;
				} else if(newSize > label.tags.Length) {
					//grow tags array
					Tag[] temp = new Tag[newSize];
					if(label.tags.Length != 0) {
						Array.Copy(label.tags, 0, temp, 0, label.tags.Length - 1);
					}
					for(int i = label.tags.Length; i < newSize; i++) {
						temp[i] = new Tag();
					}
					label.tags = temp;
				}
			}
			EditorGUILayout.Separator();

			for(int i = 0; i < label.tags.Length; ++i) {
				int newSelectedType = EditorGUILayout.Popup(0, Enum.GetNames(typeof(TagEnum)));
				label.tags[i].type = (TagEnum)Enum.GetValues(typeof(TagEnum)).GetValue(i);
				//label.tags[i].name = EditorGUILayout.TextField("Name: ", label.tags[i].name);
				label.tags[i].severity = EditorGUILayout.FloatField("Severity: ", label.tags[i].severity);
				EditorGUILayout.Separator();
			}*/
	}

	public void doOperationList(Label label) {
		operationsExpanded = EditorGUILayout.Foldout (operationsExpanded, "Operations");
		if (!operationsExpanded)
			return;
		
		for(int i=0; i<label.operations.Length; ++i)
			if (label.operations[i] == null)
				label.operations[i] = Operation.constructDefault();
		doArrayGUI(ref label.operations);
	}
	
	public void doEndeavourList(Label label) {
		endeavoursExpanded = EditorGUILayout.Foldout(endeavoursExpanded, "Endeavours");
		if (!endeavoursExpanded) {
			return;
		}

		for (int i=0; i<label.endeavours.Length; ++i) {
			if (label.endeavours [i] == null) {
				label.endeavours [i] = EndeavourFactory.constructDefault (label);
			}
		}
		doArrayGUI(ref label.endeavours);
	}

	private void doArrayGUI<T>(ref T[] array) where T:InspectorListElement {
		GUILayout.BeginHorizontal();
		int newSize = Math.Max(EditorGUILayout.IntField("Count", array.Length), 0);
		if (newSize != array.Length) {
			array = resize(array, newSize);
			return;
		}
		GUILayout.EndHorizontal();
		
		// draw list
		for(int i=0; i<array.Length; ++i) {
			GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
			GUILayout.BeginHorizontal();
			
			// draw element controls
			GUILayout.BeginVertical();
			if (GUILayout.Button("X", GUILayout.MaxWidth(20), GUILayout.MaxHeight(16))) {
				array = remove(array, i);
				--i;
				continue;
			}
			if (i < array.Length -1 && GUILayout.Button("V", GUILayout.MaxWidth(20), GUILayout.MaxHeight(16))) {
				moveDown(ref array, i);
				--i;
				continue;
			}
			GUILayout.EndVertical();

			// draw element
			GUILayout.BeginVertical();
			array[i] = (T) array[i].doListElementGUI();
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}
		
		// draw add button
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Add")) {
			array = resize(array, array.Length +1);
		}
		GUILayout.EndHorizontal();
	}

	private T[] resize<T>(T[] array, int newSize) {
		if (newSize == array.Length)
			return array;
		T[] newArray = new T[newSize];
		Array.Copy(array, newArray, Math.Min(newSize, array.Length));
		return newArray;
	}

	private T[] remove<T>(T[] array, int indexToRemove) {
		T[] newArray = new T[array.Length -1];
		Array.Copy(array, newArray, indexToRemove);
		Array.Copy(array, indexToRemove +1, newArray, indexToRemove, newArray.Length -indexToRemove);
		return newArray;
	}

	private void moveDown<T>(ref T[] array, int index) {
		T temp = array[index];
		array[index] = array[index +1];
		array[index +1] = temp;
	}
}
