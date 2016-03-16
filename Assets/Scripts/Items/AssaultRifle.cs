using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class AssaultRifle : AbstractGun {

	public float inaccuracy = 0.1f;
	public float range = 1000;
	public float damage = 10;
	public float impulse = 1;

	public Transform hitEffect;
	public float hitEffectLifetime = 3;

	public Transform robotHitEffect;
	public float robotHitEffectLifetime = 3;

	public override void onTake(Inventory taker) {
		base.onTake(taker);
		taker.equip(this);
	}

	[ClientCallback]
	void Update() {
		base.Update();
		if(cycleTime > 0)
			cycleTime -= Time.deltaTime;
		if(cycleTime <= 0 && shooting) {
			Transform cam = inventory.getPlayer().cam.transform;
			shoot(cam.position, cam.forward);
		}
	}

	[Client]
	protected override void doBullet(Vector3 position, Vector3 direction, float power) {
		if (power <= 0)
			return;
		RaycastHit hitInfo;
		bool hit = Physics.Raycast(position, direction, out hitInfo, range);
		if (hit) {

			Rigidbody rb = getParentComponent<Rigidbody>(hitInfo.transform);
			if (rb != null) {
				rb.AddForceAtPosition(direction * impulse, hitInfo.point);
			}

			RobotController controller = getParentComponent<RobotController>(hitInfo.transform);
			if(controller != null) {
				CmdApplyDamage(controller.netId, direction, hitInfo);

				// do ricochet
				//if (-Vector3.Dot(direction, hitInfo.normal) < 0.5f) {
				//	doBullet(hitInfo.point, Vector3.Reflect(direction, hitInfo.normal), power -0.25f);
				//}
				createHitEffect(robotHitEffect, robotHitEffectLifetime, hitInfo.point, hitInfo.normal);
			} else {
				createHitEffect(hitEffect, hitEffectLifetime, hitInfo.point, hitInfo.normal);
			}
		}
	}

	[Command]
	protected void CmdApplyDamage(NetworkInstanceId hit, Vector3 direction, RaycastHit hitInfo) {
		GameObject robot = ClientScene.FindLocalObject(hit);
		RobotController robotController = robot.GetComponent<RobotController>();
		NavMeshAgent navAgent = robotController.GetComponent<NavMeshAgent>();
		if(navAgent != null) {
			navAgent.speed -= 2f;
			if(navAgent.speed < 1f) {
				navAgent.speed = 1;
			}
		}
		robotController.health -= calculateDamage(direction, hitInfo);
	}

	[Server]
	protected float calculateDamage(Vector3 trajectory, RaycastHit hitInfo) {
		float multiplier = Mathf.Pow(Mathf.Max(-Vector3.Dot(trajectory, hitInfo.normal), 0), 20) *5;
        float calculatedDamage = damage *(1 +multiplier);
		return calculatedDamage;
	}

	protected T getParentComponent<T>(Transform trans) where T:UnityEngine.Object {
		T comp = trans.GetComponent<T>();
		if(comp != null)
			return comp;
		if(trans.parent != null)
			return getParentComponent<T>(trans.parent);
		return null;
	}

	protected void createHitEffect(Transform hitEffectPrefab, float lifetime, Vector3 location, Vector3 direction) {
		if (hitEffectPrefab == null)
			return;
		Transform effect = (Transform)Instantiate(hitEffectPrefab, location, Quaternion.LookRotation(direction, Vector3.up));
		effect.hideFlags |= HideFlags.HideInHierarchy;
		Destroy(effect.gameObject, lifetime);
	}
}
