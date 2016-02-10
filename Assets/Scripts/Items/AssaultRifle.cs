using UnityEngine;
using System.Collections;
using System;

public class AssaultRifle : Item {

	public float fireDelay = 0.1f;
	public float inaccuracy = 0.1f;
	public float range = 1000;
	public float damage = 10;
	public float impulse = 1;

	public Transform hitEffect;
	public float hitEffectLifetime = 3;

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

			if (hitEffect != null) {
				Transform effect = (Transform)Instantiate(hitEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal, Vector3.up));
				Destroy(effect.gameObject, hitEffectLifetime);
			}

			Rigidbody rb = getParentComponent<Rigidbody>(hitInfo.transform);
			RobotController controller = getParentComponent<RobotController>(hitInfo.transform);
			NavMeshAgent navAgent = null;
			if(controller != null) {
				navAgent = controller.GetComponent<NavMeshAgent>();
				controller.health -= 5f;
			}
			if(navAgent != null) {
				navAgent.speed -= 1f;
				if(navAgent.speed < 1f) {
					navAgent.speed = 1;
				}
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
