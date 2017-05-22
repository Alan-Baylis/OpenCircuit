using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIUtil {


	public static int dropDownSelector(Rect relativePosition, List<string> options, int startValue) {
		return GUI.SelectionGrid(convertRect(relativePosition), startValue, options.ToArray(), 2);
	}

	public static float numberField(Rect relativePosition, float startValue) {
		try {
			return float.Parse(GUI.TextField(convertRect(relativePosition), startValue.ToString()));
		} catch (System.FormatException) {
			return startValue;
		}
	}
	public static int numberField(Rect relativePosition, int startValue) {
		try {
			return int.Parse(GUI.TextField(convertRect(relativePosition), startValue.ToString()));
		} catch (System.FormatException) {
			return startValue;
		}
	}

	public static bool button(string text, Rect unconvertedRect, GUIStyle style = null) {
		if (style == null)
			return GUI.Button(convertRect (unconvertedRect), text);
		return GUI.Button(convertRect (unconvertedRect), text, style);
	}

	public static void adjustFontSize(GUIStyle style, float height) {
		style.fontSize = (int)(height *Screen.height);
	}

	public static Rect convertRect(Rect r, bool fixedHeight=false) {
		if (fixedHeight)
			return new Rect(r.x * Screen.height, r.y * Screen.height, r.width * Screen.height, r.height);
		return new Rect(r.x * Screen.height, r.y * Screen.height, r.width * Screen.height, r.height * Screen.height);
	}


	private GUIUtil() {}
}
