using UnityEngine;
using System.Collections;

[System.Serializable]
public class Guard : EndeavourFactory {

	public const float NUM_STRIPES = 8f;
	public const float LENGTH = 1f;


	public static Color COLOR_ONE = Color.black;
	public static Color COLOR_TWO = Color.yellow;

	public override Endeavour constructEndeavour(RobotController controller) {
		if(parent == null) {
			return null;
		}

		return new GuardAction(this, controller, goals, parent);
	}

#if UNITY_EDITOR
	public override void drawGizmo() {
		float sphereSize = .2f;
		for(int i = 0; i < NUM_STRIPES; i++) {
			Gizmos.color = i % 2 == 0 ? COLOR_ONE : COLOR_TWO;
			Vector3 startPos = parent.transform.position + (parent.transform.forward * (sphereSize - .02f)) + ((i * (LENGTH / NUM_STRIPES)) * parent.transform.forward);
			Vector3 endPos = startPos + (((LENGTH / NUM_STRIPES)) * parent.transform.forward);
			Gizmos.DrawLine(startPos, endPos);
		}
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(parent.transform.position, sphereSize);
	}
#endif
}
