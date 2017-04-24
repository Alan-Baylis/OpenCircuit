using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(RobotController), true)]
public class RobotControllerGUI : Editor {

    private bool status = true;
    private int size = 0;
	private Font debugFont;
	private bool endeavoursExpanded = true;

    void onEnable() {
		debugFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/GUI/Courier.ttf");
		size = serializedObject.FindProperty("goals").arraySize;
    }

	void OnSceneGUI() {
		if(!Application.isPlaying)
			return;

		Handles.BeginGUI();
		serializedObject.Update();
		RobotController robot = (RobotController)target;
		Battery battery = robot.GetComponentInChildren<Battery>();

		GUIStyle debugStyle = new GUIStyle(GUI.skin.textArea);
		debugStyle.font = debugFont;
		debugStyle.fontSize = 14;
		GUIStyle debugLabelStyle = new GUIStyle(GUI.skin.label);
		debugLabelStyle.font = debugFont;
		debugLabelStyle.fontSize = 14;

		Texture2D blue = new Texture2D(1, 1);
		Color transparentBlue = new Color(.1f, .1f, 1f, 1f);
		blue.SetPixel(0, 0, transparentBlue);
		blue.alphaIsTransparency = true;

		blue.Apply();

		Texture2D green = new Texture2D(1, 1);
		Color transparentGreen = new Color(.1f, 1f, .1f, 1f);
		green.SetPixel(0, 0, transparentGreen);
		green.alphaIsTransparency = true;

		green.Apply();

		Texture2D red = new Texture2D(1, 1);
		Color transparentRed = new Color(1f, .1f, .1f, 1f);
		red.SetPixel(0, 0, transparentRed);
		red.alphaIsTransparency = true;

		red.Apply();


		GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
		labelStyle.font = debugFont;
		labelStyle.fontSize = 14;

		GUIStyle blueStyle = new GUIStyle(GUI.skin.box);
		blueStyle.border = new RectOffset(0, 0, 0, 0);

		const float windowWidth = 240f;
		const float verticalOffset = 42f;
		Rect rectangle;
		if(battery != null) {
			rectangle = new Rect(5, 20, windowWidth, verticalOffset + 22f * robot.lines.Count + 44f);
		} else {
			rectangle = new Rect(5, 20, windowWidth, verticalOffset + 22f * robot.lines.Count);
		}
		GUILayout.Window(2, rectangle, (id) => {
			robot.shouldAlphabetize = GUILayout.Toggle(robot.shouldAlphabetize, "Alphabetize?");
			//GUILayoutOption[] options = new GUILayoutOption[] { GUILayout.Width(windowWidth)};
			for (int i = 0; i <robot.lines.Count; ++i) {
				DecisionInfoObject obj = robot.lines[i];

				GUILayoutOption[] boxOptions = {
					GUILayout.Width(100 * (obj.getPriority() / robot.maxPriority)) 
				};				

				GUILayout.BeginHorizontal();
				GUIContent label = new GUIContent(obj.getTitle().PadRight(10));

				Vector2 labelSize = labelStyle.CalcSize(label);
				//GUILayoutOption[] labelOptions = new GUILayoutOption[] { 
				//	GUILayout.Width(labelSize.x)
				//};
				float spaceBefore = 0;
				if(robot.maxPriority > 0) {
					spaceBefore = 125;
				} else {
					spaceBefore = 125 * ((robot.maxPriority + obj.getPriority()) / robot.maxPriority);
				}
				GUILayout.Space(spaceBefore);


				GUI.skin.box.normal.background = blue;
				GUILayout.Box(GUIContent.none, blueStyle, boxOptions);

				Rect checkBox = new Rect(5, i * 22 + verticalOffset, 15, 15);
				if (obj.isChosen()) {
					GUI.DrawTexture(checkBox, green);
				} else {
					GUI.DrawTexture(checkBox, red);
				}

				GUI.Label(new Rect(30, i * 22 + verticalOffset, labelSize.x, labelSize.y), label.text, labelStyle);
				GUIContent priorityContent = new GUIContent(obj.getPriority().ToString("0.#0").PadLeft(7));
				GUI.Label(new Rect(labelSize.x + 115 - labelStyle.CalcSize(priorityContent).x / 2, i * 22 + verticalOffset, 200, labelSize.y), priorityContent, labelStyle);

				if(robot.maxPriority > 0) {
					GUILayout.Space(100 * ((robot.maxPriority - obj.getPriority()) / robot.maxPriority));
				} else {
					GUILayout.Space(100);
				}
				GUILayout.EndHorizontal();
			}

			if(battery != null) {

				battery.currentCapacity = GUILayout.HorizontalSlider(battery.currentCapacity, 0, battery.maximumCapacity);
				//GUILayout.BeginHorizontal(options);
				GUIContent zero = new GUIContent("0");
				GUIContent cur = new GUIContent(battery.currentCapacity.ToString("0.#0"));
				GUIContent max = new GUIContent(battery.maximumCapacity.ToString());
				Vector2 labelSize = labelStyle.CalcSize(zero);
				GUI.Label(new Rect(5, (robot.lines.Count + 1) * 22 + verticalOffset, labelSize.x, labelSize.y), zero, labelStyle);
				labelSize = labelStyle.CalcSize(cur);
				GUI.Label(new Rect(windowWidth/2 - labelSize.x/2, (robot.lines.Count + 1) * 22 + verticalOffset, labelSize.x, labelSize.y), cur, labelStyle);
				labelSize = labelStyle.CalcSize(max);
				GUI.Label(new Rect(windowWidth - labelSize.x, (robot.lines.Count + 1) * 22 + verticalOffset, labelSize.x, labelSize.y), max, labelStyle);
			}
		}, robot.name);

		Handles.EndGUI();
	}

	public override void OnInspectorGUI() {
        serializedObject.Update();
		RobotController robot = (RobotController)target;
		EditorGUILayout.PropertyField(serializedObject.FindProperty("debug"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("reliability"));
        SerializedProperty goals = serializedObject.FindProperty("goals");
        status = UnityEditor.EditorGUILayout.Foldout(status, "Goals");
		if(status && goals != null) {

			int newSize = UnityEditor.EditorGUILayout.IntField("Size:", goals.arraySize);
			if(newSize != size) {
				if(newSize < goals.arraySize) {
					while(newSize < goals.arraySize) {
						goals.DeleteArrayElementAtIndex(goals.arraySize - 1);
					}
				} else if(newSize > goals.arraySize) {
					while(newSize > goals.arraySize) {
						goals.InsertArrayElementAtIndex(goals.arraySize);
					}
				}
				size = newSize;
			}
				EditorGUILayout.Separator();

				for(int i = 0; i < goals.arraySize; i++) {
					EditorGUILayout.PropertyField(goals.GetArrayElementAtIndex(i).FindPropertyRelative("type"));
					EditorGUILayout.PropertyField(goals.GetArrayElementAtIndex(i).FindPropertyRelative("priority"));
					EditorGUILayout.Separator();
				}
		}
        EditorGUILayout.PropertyField(serializedObject.FindProperty("locations"), true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("targetSightedSound"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("destructionEffect"), true);
		doEndeavourList(robot);
		//robot.OnBeforeSerialize();

        serializedObject.ApplyModifiedProperties();

		//finally, apply the changes
		robot.OnBeforeSerialize();
		EditorUtility.SetDirty(target);
		serializedObject.Update();
		serializedObject.ApplyModifiedProperties();
	}

	public void doEndeavourList(RobotController robot) {
		listFoldout(ref endeavoursExpanded, ref robot.endeavourFactories, "Actions");
		if(!endeavoursExpanded) {
			return;
		}

		for(int i = 0; i < robot.endeavourFactories.Length; ++i) {
			if(robot.endeavourFactories[i] == null) {
				robot.endeavourFactories[i] = EndeavourFactory.constructDefault();
			}
		}
		doArrayGUI(ref robot.endeavourFactories, robot.gameObject);
	}

	private static void doArrayGUI<T>(ref T[] array, GameObject parent) where T : InspectorListElement {
		// draw list
		for(int i = 0; i < array.Length; ++i) {
			GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
			GUILayout.BeginHorizontal();

			// draw element controls
			GUILayout.BeginVertical();
			if(GUILayout.Button("X", GUILayout.MaxWidth(20), GUILayout.MaxHeight(16))) {
				array = remove(array, i);
				--i;
				continue;
			}
			if(i < array.Length - 1 && GUILayout.Button("V", GUILayout.MaxWidth(20), GUILayout.MaxHeight(16))) {
				moveDown(ref array, i);
				--i;
				continue;
			}
			GUILayout.EndVertical();

			// draw element
			GUILayout.BeginVertical();
			array[i] = (T)array[i].doListElementGUI(parent);
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		// draw add button
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Add")) {
			array = resize(array, array.Length + 1);
		}
		GUILayout.EndHorizontal();
	}

	private static T[] remove<T>(T[] array, int indexToRemove) {
		T[] newArray = new T[array.Length - 1];
		Array.Copy(array, newArray, indexToRemove);
		Array.Copy(array, indexToRemove + 1, newArray, indexToRemove, newArray.Length - indexToRemove);
		return newArray;
	}

	private static void moveDown<T>(ref T[] array, int index) {
		T temp = array[index];
		array[index] = array[index + 1];
		array[index + 1] = temp;
	}

	private static void listFoldout<T>(ref bool expanded, ref T[] array, string label) {
		GUILayout.BeginHorizontal();
		expanded = EditorGUILayout.Foldout(expanded, label);
		int newSize = Math.Max(EditorGUILayout.IntField(array.Length), 0);
		if (newSize != array.Length)
			array = resize(array, newSize);
		GUILayout.EndHorizontal();
	}

	private static T[] resize<T>(T[] array, int newSize) {
		if(newSize == array.Length)
			return array;
		T[] newArray = new T[newSize];
		Array.Copy(array, newArray, Math.Min(newSize, array.Length));
		return newArray;
	}
}
