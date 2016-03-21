using UnityEngine;
using System.Collections;

using UnityEngine.Networking;

public abstract class AbstractGun : Item {

	public AudioClip fireSound;
	public float fireSoundVolume = 1;
	public float fireDelay = 0.1f;
	public float reloadTime = 1f;

	public int magazineSize = 20;
	public int maxMagazines = 5;
	public Vector3 recoilDistance = new Vector3(0, 0, 0.2f);
	
	public Vector3 fireEffectLocation;
	public EffectSpec fireEffect;
	public EffectSpec fireEffectSideways;
	public EffectSpec fireEffectLight;


	protected int maxBullets;
	[SyncVar]
	protected int bulletsRemaining = 5 * 20;
	[SyncVar]
	protected int currentMagazineFill;
	
	protected AudioSource audioSource;
	protected Inventory inventory;


	protected bool shooting = false;
	protected bool reloading = false;

	protected float cycleTime = 0;
	protected float reloadTimeRemaining = 0;

	void Start() {
		maxBullets = magazineSize * maxMagazines;
		currentMagazineFill = magazineSize; //one mag loaded
		bulletsRemaining = (maxMagazines - 1) * magazineSize; //the rest in reserve
	}

	[ClientCallback]
	void Update() {
		base.Update();
		if(cycleTime > 0)
			cycleTime -= Time.deltaTime;
		if(cycleTime <= 0 && shooting && !reloading) {
			Transform cam = inventory.getPlayer().cam.transform;
			shoot(cam.position, cam.forward);
		} else if(reloading) {
			reloadTimeRemaining -= Time.deltaTime;
			if(reloadTimeRemaining <= 0) {
				reloading = false;
			}
		}
	}

	public override void beginInvoke(Inventory invoker) {
		this.inventory = invoker;
		shooting = true;
	}

	public override void endInvoke(Inventory invoker) {
		base.endInvoke(invoker);
		shooting = false;
	}

	public override void onEquip(Inventory equipper) {
		base.onEquip(equipper);
		audioSource = equipper.gameObject.AddComponent<AudioSource>();
		audioSource.clip = fireSound;
		audioSource.volume = fireSoundVolume;
	}

	public override void onUnequip(Inventory equipper) {
		base.onUnequip(equipper);
		if(audioSource != null)
			Destroy(audioSource);
	}

	public bool addMags(int number) {
		int currentAmmo = bulletsRemaining + currentMagazineFill;
		if(currentAmmo < maxBullets) {
			int bulletsNeeded = maxBullets - currentAmmo;
			bulletsRemaining += (bulletsNeeded < magazineSize * number) ? bulletsNeeded : magazineSize * number;
			return true;
		} else {
			return false;
		}

	}

	public void reload() {
		reloading = true;
		int bulletsNeeded = magazineSize - currentMagazineFill;
		if(bulletsRemaining > bulletsNeeded) {
			currentMagazineFill += bulletsNeeded;
			bulletsRemaining -= bulletsNeeded;
		} else {
			currentMagazineFill += bulletsRemaining;
			bulletsRemaining = 0;
		}
		reloadTimeRemaining += reloadTime; 
	}

	protected void shoot(Vector3 position, Vector3 direction) {
		if(currentMagazineFill > 0) {
			audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
			audioSource.Play();
			doBullet(position, direction, 1);
			cycleTime += fireDelay;
			--currentMagazineFill;
			transform.position -= transform.TransformVector(recoilDistance);
			
			// do fire effects
			Vector3 effectPosition = transform.TransformPoint(fireEffectLocation);
			fireEffect.spawn(effectPosition, -transform.forward);
			fireEffectSideways.spawn(effectPosition, -transform.right -transform.forward);
			fireEffectSideways.spawn(effectPosition, transform.right -transform.forward);
			fireEffectLight.spawn(effectPosition);
		} else {
			reload();
		}
	}

	protected abstract void doBullet(Vector3 position, Vector3 direction, float power);

}
