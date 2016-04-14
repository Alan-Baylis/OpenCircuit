using UnityEngine;
using System.Collections;

[AddComponentMenu("Scripts/Items/Flashlight")]
public class Flashlight : Item {

	public float range = 40;
	public float angle = 60;
	public float intensity = 1;
	public Color color = Color.white;

	protected Light lightComp = null;

	public override void beginInvoke(Inventory invoker) {
		if (lightComp == null || lightComp.enabled)
			turnOff();
		else
			turnOn();
    }

	public override void onEquip(Inventory equipper) {
		base.onEquip(equipper);
		createLight(equipper.getPlayer().cam.gameObject);
		turnOn();
	}

	public override void onUnequip(Inventory equipper) {
		base.onUnequip(equipper);
		deleteLight();
	}

	protected void turnOn() {
		lightComp.enabled = true;
	}

	protected void turnOff() {
		if (lightComp == null)
			return;
		lightComp.enabled = false;
	}

	protected void createLight(GameObject target) {
		deleteLight();
		lightComp = target.AddComponent<Light>();
		lightComp.enabled = false;
		lightComp.type = LightType.Spot;
		lightComp.range = range;
		lightComp.spotAngle = angle;
		lightComp.color = color;
		lightComp.intensity = intensity;
//		lightComp.bounceIntensity = 1;
		lightComp.shadows = LightShadows.Soft;
//		lightComp.shadowStrength = 1
		lightComp.renderMode = LightRenderMode.Auto;
	}

	protected void deleteLight() {
		if (lightComp == null)
			return;
		GameObject.Destroy(lightComp);
		lightComp = null;
	}
}
