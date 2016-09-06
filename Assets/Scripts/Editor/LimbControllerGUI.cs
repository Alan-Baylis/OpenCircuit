using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(LegController), true)]
public class LimbControllerGUI : Editor {

	bool posing = false;
	Vector3 targetPos = Vector3.zero;

	public void OnSceneGUI() {
		if (posing) {
			targetPos = UnityEditor.Handles.PositionHandle(targetPos, Quaternion.identity);
		}
		((LegController)target).setPosition(targetPos);
	}

	public override void OnInspectorGUI() {

		LegController controller = (LegController)target;

		if (posing) {
			posing = !GUILayout.Button("Stop Posing");
			if (GUILayout.Button("Reset"))
				targetPos = controller.getDefaultPos();
		} else {
			posing = GUILayout.Button("Start Posing");
			if (posing)
				targetPos = controller.deducePosition();
		}
		GUILayout.Space(10);

        base.DrawDefaultInspector();

		((LegController)target).setPosition(targetPos);
	}

}
