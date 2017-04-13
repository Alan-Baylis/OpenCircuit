using UnityEngine;

[System.Serializable]
public class AttackRoute : AbstractRouteTag {

	public AttackRoute(float severity, LabelHandle handle) : base(TagEnum.AttackRoute, severity, handle) {
	}

	public Vector3 getEndPoint() {
		return getPointHandles()[getPointHandles().Count - 1].getPosition();
	}
}