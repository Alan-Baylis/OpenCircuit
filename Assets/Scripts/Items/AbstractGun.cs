using UnityEngine;
using System.Collections;

using UnityEngine.Networking;

public abstract class AbstractGun : Item {

	public AudioClip fireSound;
	public AudioClip reloadSound;

	public float fireSoundVolume = 1;
	public float fireDelay = 0.1f;

	public float reloadTime = 1f;
	public int magazineSize = 20;
	public int maxMagazines = 5;

	public float baseInaccuracy = 0.1f;
	public float maximumMovementInaccuracy = 0.1f;
	public float movementInaccuracySoftness = 10f;
	public Vector3 recoilAnimationDistance = new Vector3(0, 0, 0.2f);
	public Vector2 recoilMinRotation = new Vector3(-0.1f, 0.1f);
	public Vector2 recoilMaxRotation = new Vector3(0.1f, 0.2f);
	public HoldPosition reloadPosition;

	public Vector3 fireEffectLocation;
	public EffectSpec fireEffect;
	public EffectSpec fireEffectSideways;
	public EffectSpec fireEffectLight;

	public GUISkin guiSkin;
	//public Texture bulletIcon;

	protected int maxBullets;
	[SyncVar]
	protected int bulletsRemaining = 5 * 20;
	[SyncVar]
	protected int currentMagazineFill;
	
	protected Inventory inventory;


	protected bool shooting = false;
	protected bool reloading = false;

	protected float cycleTime = 0;
	protected float reloadTimeRemaining = 0;

	private AudioSource soundEmitter;
	private Texture2D myBlackTexture = null;
	private Texture2D blackTexture {
		get {
			if (myBlackTexture == null) {
				myBlackTexture = new Texture2D(1, 1);
				myBlackTexture.SetPixel(0, 0, Color.black);
			}
			return myBlackTexture;
		}
	}

	void Start() {
		maxBullets = magazineSize * maxMagazines;
		currentMagazineFill = magazineSize; //one mag loaded
		bulletsRemaining = (maxMagazines - 1) * magazineSize; //the rest in reserve
	}

	[ClientCallback]
	public override void Update() {
		base.Update();
		if(cycleTime > 0)
			cycleTime -= Time.deltaTime;
		if(cycleTime <= 0 && shooting && !reloading) {
			Transform cam = inventory.getPlayer().cam.transform;
			shoot(cam.position, cam.forward);
			MouseLook looker = inventory.getPlayer().looker;
			looker.rotate(
				Random.Range(recoilMinRotation.x, recoilMaxRotation.x),
				Random.Range(recoilMinRotation.y, recoilMaxRotation.y));
		} else if(reloading) {
			reloadTimeRemaining -= Time.deltaTime;
			if(reloadTimeRemaining <= 0) {
				reloading = false;
			}
		}
	}

	[ClientCallback]
	void OnGUI() {
		//GUI.Window(0, new Rect(GUI., 5, 50, 50), windowFunction, GUIContent.none);
		if(transform.parent != null && transform.parent.GetComponent<Player>().isLocalPlayer) {

			GUI.skin = guiSkin;

			int padding = 10;
			int boxWidth = 80 +padding *3;
			int boxHeight = 30 +padding *2;
			int boxCornerX = Screen.width - boxWidth - padding;
			int boxCornerY = Screen.height - boxHeight - padding;
			GUI.color = new Color(0, 0, 0, 0.6f);
			GUI.DrawTexture(new Rect(boxCornerX -2, boxCornerY -2, boxWidth +4, boxHeight +4), blackTexture);
			GUI.color = Color.white;
			GUI.Box(new Rect(boxCornerX, boxCornerY, boxWidth, boxHeight), GUIContent.none);

			//int imageWidth = 50;
			//int imageHeight = 50;
			//GUI.DrawTexture(new Rect(boxCornerX + padding, boxCornerY + padding * 2 + 20, imageWidth, imageHeight), bulletIcon);
			GUI.TextArea(new Rect(boxCornerX + padding *2, boxCornerY + padding, 60, 40), "" + Mathf.Ceil((float)bulletsRemaining / (float)magazineSize));
			GUI.TextArea(new Rect(boxCornerX + padding *2 +40, boxCornerY + padding, 60, 40), "" + currentMagazineFill);
		}
	}

	public override HoldPosition getHoldPosition() {
		if (reloading)
			return reloadPosition;
		return base.getHoldPosition();
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
		getAudioSource().clip = fireSound;
		getAudioSource().volume = fireSoundVolume;
	}

	public override void onUnequip(Inventory equipper) {
		base.onUnequip(equipper);
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
		if(currentMagazineFill < magazineSize) {
			reloading = true;
			getAudioSource().PlayOneShot(reloadSound);
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
	}

	public AudioSource getAudioSource() {
		if(soundEmitter == null) {
			soundEmitter = GetComponent<AudioSource>();
		}
		return soundEmitter;
	}

	protected void shoot(Vector3 position, Vector3 direction) {
		if(currentMagazineFill > 0) {

			direction = inaccurateDirection(direction, getMovementInaccuracy());
            direction = inaccurateDirection(direction, baseInaccuracy);
            doBullet(position, direction, 1);
			cycleTime += fireDelay;
			--currentMagazineFill;
			transform.position -= transform.TransformVector(recoilAnimationDistance);

			doFireEffects();
		} else {
			reload();
		}
	}

	protected void doFireEffects() {
		getAudioSource().pitch = UnityEngine.Random.Range(0.95f, 1.05f);
		getAudioSource().Play();

		// do fire effects
		Vector3 effectPosition = transform.TransformPoint(fireEffectLocation);
		fireEffect.spawn(effectPosition, -transform.forward);
		fireEffectSideways.spawn(effectPosition, -transform.right - transform.forward);
		fireEffectSideways.spawn(effectPosition, transform.right - transform.forward);
		fireEffectLight.spawn(effectPosition);
	}

	protected virtual float getMovementInaccuracy() {
		// here we use a rational function to get the desired behaviour
		const float arbitraryValue = 0.2f; // the larger this value is, the faster the player must be moving before it affects his accuracy
		float speed = inventory.GetComponent<Rigidbody>().velocity.magnitude;
		float inaccuracy = (maximumMovementInaccuracy * speed -arbitraryValue) / (speed +movementInaccuracySoftness);
		return Mathf.Max(inaccuracy, 0);
	}

	protected abstract void doBullet(Vector3 position, Vector3 direction, float power);

	public static Vector3 inaccurateDirection(Vector3 direction, float inaccuracy) {
		Vector3 randomAngle = Random.onUnitSphere;
		float angle = Vector3.Angle(direction, randomAngle) /360;
		return Vector3.RotateTowards(direction, Random.onUnitSphere, Mathf.PI *angle *inaccuracy, 0);
	}
}
