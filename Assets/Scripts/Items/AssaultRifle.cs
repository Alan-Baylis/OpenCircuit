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

	public Transform robotHitEffect;
	public float robotHitEffectLifetime = 3;

	public AudioClip fireSound;
	public float fireSoundVolume = 1;

	protected Inventory inventory;
	protected bool shooting = false;
	protected float cycleTime = 0;
	protected AudioSource audioSource;

	public override void beginInvoke(Inventory invoker) {
		this.inventory = invoker;
		shooting = true;
	}

	public override void onEquip(Inventory equipper) {
		base.onEquip(equipper);
		equipper.StartCoroutine(this.cycle());
		audioSource = equipper.gameObject.AddComponent<AudioSource>();
		audioSource.clip = fireSound;
		audioSource.volume = fireSoundVolume;
    }

	public override void onUnequip(Inventory equipper) {
		base.onUnequip(equipper);
		equipper.StopCoroutine(this.cycle());
		if (audioSource != null)
			Destroy(audioSource);
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
		audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
		audioSource.Play();
		Transform cam = inventory.getPlayer().cam.transform;
		doBullet(cam.position, cam.forward, 1);
		cycleTime += fireDelay;
	}

	protected void doBullet(Vector3 position, Vector3 direction, float power) {
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
				NavMeshAgent navAgent = controller.GetComponent<NavMeshAgent>();
				if (navAgent != null) {
					navAgent.speed -= 2f;
					if (navAgent.speed < 1f) {
						navAgent.speed = 1;
					}
				}
				controller.health -= calculateDamage(direction, hitInfo);

				// do ricochet
				if (-Vector3.Dot(direction, hitInfo.normal) < 0.5f) {
					doBullet(hitInfo.point, Vector3.Reflect(direction, hitInfo.normal), power -0.25f);
				}

				if (robotHitEffect != null) {
					Transform robotEffect = (Transform)Instantiate(robotHitEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal, Vector3.up));
					Destroy(robotEffect.gameObject, robotHitEffectLifetime);
				}
			} else if (hitEffect != null) {
				Transform effect = (Transform)Instantiate(hitEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal, Vector3.up));
				Destroy(effect.gameObject, hitEffectLifetime);
			}
		}
	}

	protected float calculateDamage(Vector3 trajectory, RaycastHit hitInfo) {
		float multiplier = Mathf.Pow(Mathf.Max(-Vector3.Dot(trajectory, hitInfo.normal), 0), 20) *5;
        float calculatedDamage = damage *(1 +multiplier);
		print("Calculated Damage: " +calculatedDamage);
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
		Transform effect = (Transform)Instantiate(hitEffectPrefab, location, Quaternion.LookRotation(direction, Vector3.up));
		effect.hideFlags |= HideFlags.HideInHierarchy;
		Destroy(effect.gameObject, lifetime);
	}
}
