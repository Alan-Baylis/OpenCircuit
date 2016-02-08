using UnityEngine;
using System.Collections;
using System;

public class AssaultRifle : Item {

	public float fireDelay = 0.1f;
	public float inaccuracy = 0.1f;
	public float range = 1000;
	public float damage = 10;
	public float impulse = 1;

	protected Inventory inventory;
	protected bool shooting = false;
	protected float cycleTime = 0;

	public override void beginInvoke(Inventory invoker) {
		this.inventory = invoker;
		shooting = true;
	}

	public override void onEquip(Inventory equipper) {
		base.onEquip(equipper);
		equipper.StartCoroutine(this.cycle());
	}

	public override void onUnequip(Inventory equipper) {
		base.onUnequip(equipper);
		equipper.StopCoroutine(this.cycle());
	}

	public override void endInvoke(Inventory invoker) {
		base.endInvoke(invoker);
		shooting = false;
	}

	public override void onTake(Inventory taker) {
		base.onTake(taker);
		taker.equip(this);
	}

	protected IEnumerator cycle() {
		while (true) {
			if (cycleTime > 0)
				cycleTime -= Time.deltaTime;
			if (cycleTime <= 0) {
				while(!shooting)
					yield return new WaitForFixedUpdate();
				shoot();
			}
			yield return new WaitForFixedUpdate();
		}
	}

	protected void shoot() {
		Transform cam = inventory.getPlayer().cam.transform;
		RaycastHit hitInfo;
		bool hit = Physics.Raycast(cam.position, cam.forward, out hitInfo, range);
		if (hit) {

			Rigidbody rb = getParentComponent<Rigidbody>(hitInfo.transform);
			RobotController controller = getParentComponent<RobotController>(hitInfo.transform);
			if(controller != null) {

				controller.health -= 10f;
			}
			if (rb != null) {
				rb.AddForceAtPosition(cam.forward * impulse, hitInfo.point);
			}
		}
		cycleTime += fireDelay;
	}

	protected T getParentComponent<T>(Transform trans) where T:UnityEngine.Object {
		T comp = trans.GetComponent<T>();
		if(comp != null)
			return comp;
		if(trans.parent != null)
			return getParentComponent<T>(trans.parent);
		return null;
	}
}
