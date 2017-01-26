using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Scripts/Items/BuildTool")]
public class BuildTool : ContextItem {

	public float range = 20;
	public GameObject structureBase;

	public override void beginInvoke(Inventory invoker) {
		Transform cam = holder.getPlayer().cam.transform;

		RaycastHit hitInfo;
		if (Physics.Raycast (cam.position, cam.forward, hitInfo, range)) {
			Instantiate(structureBase, hitInfo.point);
			invoker.popContext(this);
		}
	}

}
