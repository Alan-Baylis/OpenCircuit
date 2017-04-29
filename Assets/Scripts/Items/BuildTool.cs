using UnityEngine;
using UnityEngine.Networking;

[AddComponentMenu("Scripts/Items/BuildTool")]
public class BuildTool : ContextItem {

	public float range = 20;
	public Vector3 offset;
	public GameObject structureBase;
	public Material ghostMaterial;
	public float minFlatness;

	private GameObject ghost;
	private Material currentGhostMaterial;
	private bool canBuild;

	public override void Update() {
		base.Update();
		canBuild = false;

		if (ghost == null)
			return;
		if (holder == null) {
			destroyGhost();
			return;
		}

		Transform cam = holder.getPlayer().cam.transform;
		RaycastHit hitInfo;
		if (Physics.Raycast(cam.position, cam.forward, out hitInfo, range)) {
			if (!ghost.activeSelf)
				ghost.SetActive(true);
			UnityEngine.AI.NavMeshHit navHit;
			if (UnityEngine.AI.NavMesh.SamplePosition (hitInfo.point, out navHit, 0.5f, UnityEngine.AI.NavMesh.AllAreas)) {
				canBuild = navHit.normal.y >= minFlatness;
				ghost.transform.position = navHit.position + offset;
			} else {
				ghost.transform.position = hitInfo.point + offset;
			}
		} else {
			if (ghost.activeSelf)
				ghost.SetActive(false);
		}
		currentGhostMaterial.SetColor("_EmissionColor", canBuild ? Color.green : Color.red);
	}

	public void OnDestroy() {
		destroyGhost();
	}

	public override void onEquip(Inventory equipper) {
		base.onEquip(equipper);
		buildGhost();
	}

	public override void onUnequip(Inventory equipper) {
		base.onUnequip(equipper);
		destroyGhost();
		if (holder.getPlayer().isLocalPlayer)
			HUD.hud.clearFireflyElement("buildToolUsageMessage");
	}

	public override void beginInvoke(Inventory invoker) {
		TeamId team = holder.GetComponent<TeamId>();
		if (team != null && team.enabled && GlobalConfig.globalConfig.gamemode is Bases
			&& canBuild) {
			CmdSpawnTower(ghost.transform.position);
			invoker.popContext(typeof(BuildTool));
		}
	}

	[Command]
	private void CmdSpawnTower(Vector3 location) {
		Bases bases = GlobalConfig.globalConfig.gamemode as Bases;
		if (bases != null && bases.canBuildTower(holder.getPlayer().clientController)) {
			TeamId team = holder.GetComponent<TeamId>();
			CentralRobotController crc = ((Bases) GlobalConfig.globalConfig.gamemode).getCRC(team.id);
			Label towerBase = Instantiate(structureBase, location, Quaternion.identity).GetComponent<Label>();
			towerBase.enabled = true;
			crc.sightingFound(towerBase.labelHandle, towerBase.transform.position, null);
			(towerBase.getTag(TagEnum.BuildDirective) as BuildDirectiveTag).owner = holder.getPlayer().clientController;
			NetworkServer.Spawn(towerBase.gameObject);
			bases.spendBuildPoint(holder.getPlayer().clientController, towerBase.gameObject);
		}
	}

	private void buildGhost() {
		if (!holder.getPlayer().isLocalPlayer)
			return;
		destroyGhost();
		ghost = Instantiate(structureBase);
		currentGhostMaterial = new Material(ghostMaterial);
		setGhostMaterial(ghost.transform);
		Fireflies.Config config = HUD.hud.fireflyConfig;
		config.fireflySize *= 0.5f;
		HUD.hud.setFireflyElementConfig("buildToolUsageMessage", config);
		HUD.hud.setFireflyElement("buildToolUsageMessage", this,
			FireflyFont.getString("click to\nbuild tower", 0.05f, new Vector2(0, -0.25f), FireflyFont.HAlign.CENTER), true);
	}

	private void destroyGhost() {
		if (ghost != null)
			Destroy(ghost);
	}

	private void setGhostMaterial(Transform transform) {
		Renderer renderer = transform.GetComponent<Renderer>();
		if (renderer != null)
			renderer.material = currentGhostMaterial;
		for(int i=0; i<transform.childCount; ++i) {
			setGhostMaterial(transform.GetChild(i));
		}
	}

}
