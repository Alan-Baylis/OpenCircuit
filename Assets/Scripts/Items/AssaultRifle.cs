using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class AssaultRifle : AbstractGun {

	public float inaccuracy = 0.1f;
	public float range = 1000;
	public float damage = 10;
	public float impulse = 1;

	public EffectSpec hitEffect;
	public EffectSpec robotHitEffect;

	public override void onTake(Inventory taker) {
		base.onTake(taker);
		taker.equip(this);
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

			Health health = getParentComponent<Health>(hitInfo.transform);
			if(health != null) {
				CmdApplyDamage(health.netId, direction, hitInfo);

				// do ricochet
				//if (-Vector3.Dot(direction, hitInfo.normal) < 0.5f) {
				//	doBullet(hitInfo.point, Vector3.Reflect(direction, hitInfo.normal), power -0.25f);
				//}
				robotHitEffect.spawn(hitInfo.point, hitInfo.normal);
			} else {
				hitEffect.spawn(hitInfo.point, hitInfo.normal);
			}
		}
	}

	[Command]
	protected void CmdApplyDamage(NetworkInstanceId hit, Vector3 direction, RaycastHit hitInfo) {
		GameObject hitObject = ClientScene.FindLocalObject(hit);
		Health health = hitObject.GetComponent<Health>();
		NavMeshAgent navAgent = hitObject.GetComponent<NavMeshAgent>();
		if(navAgent != null) {
			navAgent.speed -= 2f;
			if(navAgent.speed < 1f) {
				navAgent.speed = 1;
			}
			navAgent.baseOffset -= 0.1f;
			if (navAgent.baseOffset < 1.5f) {
				navAgent.baseOffset = 1.5f;
			}
		}
		if(health != null) {
			health.hurt(calculateDamage(direction, hitInfo));
		}
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

	[Client]
	protected void createHitEffect(Transform hitEffectPrefab, float lifetime, Vector3 location, Vector3 direction) {
		if (hitEffectPrefab == null)
			return;
		Transform effect = (Transform)Instantiate(hitEffectPrefab, location, Quaternion.LookRotation(direction, Vector3.up));
		effect.hideFlags |= HideFlags.HideInHierarchy;
		Destroy(effect.gameObject, lifetime);
	}
}
