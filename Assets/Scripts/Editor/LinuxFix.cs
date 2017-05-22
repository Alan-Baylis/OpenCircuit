using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class LinuxFix : EditorWindow {
	[MenuItem("Window/LinuxCPUFix")]
	public static void ShowWindow() {
		LinuxFix window = (LinuxFix)GetWindow(typeof(LinuxFix));
		window.minSize = Vector2.zero;
	}

	void Update() {
		// this allows it to be even smaller... probably a hack, but the whole thing is
		minSize = Vector2.zero;
		Repaint();
	}
}