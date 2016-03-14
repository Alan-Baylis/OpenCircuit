using UnityEngine;
using System.Collections;

public abstract class AbstractGun : Item {

	public float fireDelay = 0.1f;
	public AudioClip fireSound;

	public int magazineSize = 20;
	public int maxMagazines = 5;

	protected int maxBullets;
	protected int bulletsRemaining = 5 * 20;
	protected int currentMagazineFill;
	
	protected AudioSource audioSource;
	protected Inventory inventory;

	protected bool shooting = false;

	protected float cycleTime = 0;

	void Start() {
		print("calling start on item");
		maxBullets = magazineSize * maxMagazines;
		currentMagazineFill = magazineSize; //one mag loaded
		bulletsRemaining = (maxMagazines - 1) * magazineSize; //the rest in reserve
	}

	public override void onEquip(Inventory equipper) {
		base.onEquip(equipper);
		maxBullets = magazineSize * maxMagazines;
		currentMagazineFill = magazineSize; //one mag loaded
		bulletsRemaining = (maxMagazines - 1) * magazineSize; //the rest in reserve
	}

	public override void beginInvoke(Inventory invoker) {
		this.inventory = invoker;
		shooting = true;
	}

	protected void shoot(Vector3 position, Vector3 direction) {
		if(bulletsRemaining > 0) {
			audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
			audioSource.Play();
			doBullet(position, direction, 1);
			cycleTime += fireDelay;
			--bulletsRemaining;
		}
	}

	protected abstract void doBullet(Vector3 position, Vector3 direction, float power);

}
