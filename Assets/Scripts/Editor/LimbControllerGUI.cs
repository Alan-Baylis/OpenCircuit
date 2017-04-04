using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LimbController), true)]
public class LimbControllerGUI : Editor {

	bool posing;
	Vector3 targetPos = Vector3.zero;

	public void OnSceneGUI() {
		if (posing) {
			targetPos = Handles.PositionHandle(targetPos, Quaternion.identity);
			((LimbController)target).setPosition(targetPos);
		}
	}

	public override void OnInspectorGUI() {

		LimbController controller = (LimbController)target;

		if (posing) {
			posing = !GUILayout.Button("Stop Posing");
			if (GUILayout.Button("Reset"))
				targetPos = controller.getDefaultPos();
			controller.setPosition(targetPos);
		} else {
			posing = GUILayout.Button("Start Posing");
			if (posing)
				targetPos = controller.deducePosition();
		}
		GUILayout.Space(10);

        base.DrawDefaultInspector();

		controller.init();
	}

}
