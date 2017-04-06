using UnityEngine;

[AddComponentMenu("Scripts/Items/BuildTool")]
public class BuildTool : ContextItem {

	public float range = 20;
	public GameObject structureBase;

	public override void beginInvoke(Inventory invoker) {
		Team team = holder.GetComponent<Team>();

		if (team != null && team.enabled && GlobalConfig.globalConfig.gamemode is Bases) {
			CentralRobotController crc = ((Bases) GlobalConfig.globalConfig.gamemode).getCRC(team.team.Id);
			Transform cam = holder.getPlayer().cam.transform;

			RaycastHit hitInfo;
			if (Physics.Raycast(cam.position, cam.forward, out hitInfo, range)) {
				Label towerBase =
					(Instantiate(structureBase, hitInfo.point, Quaternion.identity)).GetComponent<Label>();
				crc.sightingFound(towerBase.labelHandle, towerBase.transform.position,
					null);
				invoker.popContext(GetType());
			}
		}
	}

}
