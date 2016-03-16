using UnityEngine;
using System.Collections;

public abstract class AbstractGun : Item {

	public AudioClip fireSound;
	public float fireSoundVolume = 1;
	public float fireDelay = 0.1f;

	public int magazineSize = 20;
	public int maxMagazines = 5;
	public Vector3 recoilDistance = new Vector3(0, 0, 0.2f);


	protected int maxBullets;
	protected int bulletsRemaining = 5 * 20;
	protected int currentMagazineFill;
	
	protected AudioSource audioSource;
	protected Inventory inventory;


	protected bool shooting = false;

	protected float cycleTime = 0;

	void Start() {
		maxBullets = magazineSize * maxMagazines;
		currentMagazineFill = magazineSize; //one mag loaded
		bulletsRemaining = (maxMagazines - 1) * magazineSize; //the rest in reserve
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

	protected void shoot(Vector3 position, Vector3 direction) {
		if(bulletsRemaining > 0) {
			audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
			audioSource.Play();
			doBullet(position, direction, 1);
			cycleTime += fireDelay;
			--bulletsRemaining;
			transform.position -= transform.TransformVector(recoilDistance);
		}
	}

	protected abstract void doBullet(Vector3 position, Vector3 direction, float power);

}
