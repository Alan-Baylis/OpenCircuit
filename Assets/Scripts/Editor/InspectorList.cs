using UnityEngine;
using UnityEditor;
using System;

public class InspectorList {
	
	public static void doArrayGUIPolymorphism<T>(ref T[] array, GameObject gameObject) where T:InspectorListElement {
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
//			array = doElementControls(array, i);
			
			// draw element
			GUILayout.BeginVertical();
			array[i] = (T) array[i].doListElementGUI(gameObject);
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
	
	public static void doArrayGUISimple(ref SerializedProperty array) {
		GUILayout.BeginHorizontal();
		int newSize = Math.Max(EditorGUILayout.IntField("Count", array.arraySize), 0);
		if (newSize != array.arraySize) {
			array.arraySize = newSize;
			return;
		}
		GUILayout.EndHorizontal();
		
		// draw list
		for(int i=0; i<array.arraySize; ++i) {
			GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
			GUILayout.BeginHorizontal();
			
			// draw element controls
			i = doElementControls(ref array, i);
			
			// draw element
			GUILayout.BeginVertical();
			/*
			 * This is a band-aid to prevent an out-of-bounds exception because 
			 * doElementControls may modify array -Brian
			 */
			if (i < array.arraySize) {
				SerializedProperty element = array.GetArrayElementAtIndex(i);
				foreach(SerializedProperty prop in element) {
					EditorGUILayout.PropertyField(prop, true);
				}
			}
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}
		
		// draw add button
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Add")) {
			array.arraySize = array.arraySize +1;
		}
		GUILayout.EndHorizontal();
		
	}

	private static T[] doElementControls<T>(T[] array, int elementIndex) {
		GUILayout.BeginVertical();
		if (GUILayout.Button("X", GUILayout.MaxWidth(20), GUILayout.MaxHeight(16))) {
			array = remove(array, elementIndex);
			--elementIndex;
		}
		else if (elementIndex < array.Length -1 && GUILayout.Button("V", GUILayout.MaxWidth(20), GUILayout.MaxHeight(16))) {
			moveDown(ref array, elementIndex);
			--elementIndex;
		}
		GUILayout.EndVertical();
		return array;
	}
	
	private static int doElementControls(ref SerializedProperty array, int elementIndex) {
		GUILayout.BeginVertical();
		if (GUILayout.Button("X", GUILayout.MaxWidth(20), GUILayout.MaxHeight(16))) {
			array.DeleteArrayElementAtIndex(elementIndex);
		}
		else if (elementIndex < array.arraySize -1 && GUILayout.Button("V", GUILayout.MaxWidth(20), GUILayout.MaxHeight(16))) {
			array.MoveArrayElement(elementIndex, elementIndex +1);
		}
		GUILayout.EndVertical();
		return elementIndex;
	}
	
	private static T[] resize<T>(T[] array, int newSize) {
		if (newSize == array.Length)
			return array;
		T[] newArray = new T[newSize];
		Array.Copy(array, newArray, Math.Min(newSize, array.Length));
		return newArray;
	}
	
	private static T[] remove<T>(T[] array, int indexToRemove) {
		T[] newArray = new T[array.Length -1];
		Array.Copy(array, newArray, indexToRemove);
		Array.Copy(array, indexToRemove +1, newArray, indexToRemove, newArray.Length -indexToRemove);
		return newArray;
	}
	
	private static void moveDown<T>(ref T[] array, int index) {
		T temp = array[index];
		array[index] = array[index +1];
		array[index +1] = temp;
	}
}
