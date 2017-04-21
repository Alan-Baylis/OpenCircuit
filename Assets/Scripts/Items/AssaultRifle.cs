using UnityEngine;
using UnityEngine.Networking;

public class AssaultRifle : AbstractGun {

    public AudioClip[] fireSounds;

	public float inaccuracy = 0.1f;
	public float range = 1000;
	public float damage = 10;
	public float impulse = 1;

	public AbstractEffectController hitEffectPrefab;
	public AbstractEffectController robotHitEffectPrefab;

	public float reloadTime = 1f;
	public int magazineSize = 20;
	public int maxMagazines = 5;
	public int bulletsRemainingDisplay = 0;
	public override void onTake(Inventory taker) {
		base.onTake(taker);
		taker.equip(this);
	}

	private int maxBullets;
	[SyncVar]
	private int bulletsRemaining = 5 * 20;
	private int currentMagazineFill;

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

	public override void Update() {
		base.Update();
		bulletsRemainingDisplay = bulletsRemaining;
	}

	void Start() {
		maxBullets = magazineSize * maxMagazines;
		currentMagazineFill = magazineSize; //one mag loaded
		bulletsRemaining = (maxMagazines) * magazineSize; //the rest in reserve
	}

	[Server]
	// The assault rifle ammo unit is magazines
	public override bool addAmmo(int quantity) {
		if (bulletsRemaining < maxBullets) {
			int bulletsNeeded = maxBullets - bulletsRemaining;
			bulletsRemaining += (bulletsNeeded < magazineSize * quantity) ? bulletsNeeded : magazineSize * quantity; 
			return true;
		} else {
			return false;
		}
	}

	public override void reload() {
		if (currentMagazineFill < magazineSize) {
			reloading = true;
			int bulletsNeeded = magazineSize - currentMagazineFill;
			if (bulletsRemaining > bulletsNeeded) {
				currentMagazineFill += bulletsNeeded;
			} else {
				currentMagazineFill += bulletsRemaining;
			}
			reloadTimeRemaining += reloadTime;
		}
	}

	[Client]
	protected override void doBullet(Vector3 position, Vector3 direction, float power) {
		if (power <= 0)
			return;
		RaycastHit hitInfo;
		bool hit = Physics.Raycast(position, direction, out hitInfo, range);
		if (hit) {

			Label label = getParentComponent<Label>(hitInfo.collider.transform);
			if (label != null) {
				NetworkIdentity networkIdentity = label.GetComponent<NetworkIdentity>();
				if (networkIdentity != null) {
					CmdBulletHitLabel(direction, hitInfo.point, hitInfo.normal, networkIdentity.netId);

					Rigidbody rb = label.GetComponent<Rigidbody>();
					if (rb != null) {
						rb.AddForceAtPosition(direction * impulse, hitInfo.point);
					}

					// do ricochet
					//if (-Vector3.Dot(direction, hitInfo.normal) < 0.5f) {
					//	doBullet(hitInfo.point, Vector3.Reflect(direction, hitInfo.normal), power -0.25f);
					//}
					GlobalConfig.globalConfig.effectsManager.spawnEffect(robotHitEffectPrefab, hitInfo.point,
						Vector3.Reflect(direction, hitInfo.normal));
				} else {
					CmdBulletHit(direction, hitInfo.point, hitInfo.normal);
					GlobalConfig.globalConfig.effectsManager.spawnEffect(hitEffectPrefab, hitInfo.point, hitInfo.normal);
				}
			} else {
				CmdBulletHit(direction, hitInfo.point, hitInfo.normal);
				GlobalConfig.globalConfig.effectsManager.spawnEffect(hitEffectPrefab, hitInfo.point, hitInfo.normal);
			}
		} else {
			CmdBulletMiss(direction);
		}
	}

	protected override bool isLoaded() {
		return currentMagazineFill > 0;
	}

	protected override void consumeAmmo() {
		if (hasAuthority) {
			--currentMagazineFill;
		}
		--bulletsRemaining;
	}

	[Server]
	protected override void applyDamage(NetworkInstanceId hit, Vector3 direction, Vector3 normal) {
		GameObject hitObject = ClientScene.FindLocalObject(hit);
		Label label = hitObject.GetComponent<Label>();
		UnityEngine.AI.NavMeshAgent navAgent = hitObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
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
		if(label != null && label.enabled) {
			label.sendTrigger(gameObject, new DamageTrigger(calculateDamage(direction, normal)));
			//health.hurt(calculateDamage(direction, normal));
		}
	}

	[Server]
	protected float calculateDamage(Vector3 trajectory, Vector3 normal) {
		float multiplier = Mathf.Pow(Mathf.Max(-Vector3.Dot(trajectory, normal), 0), 20) *5;
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

	[ClientRpc]
	protected override void RpcCreateShotEffect(HitEffectType type, Vector3 location, Vector3 direction, Vector3 normal) {
		if (!hasAuthority) {
			if (type == HitEffectType.DEFAULT) {
				GlobalConfig.globalConfig.effectsManager.spawnEffect(hitEffectPrefab, location, Vector3.Reflect(direction, normal));
			} else if (type == HitEffectType.ROBOT) {
				GlobalConfig.globalConfig.effectsManager.spawnEffect(robotHitEffectPrefab, location, Vector3.Reflect(direction, normal));
			}
			doFireEffects();
		}
	}

	[ClientRpc]
	protected override void RpcCreateFireEffects() {
		doFireEffects();
	}

	protected override void doFireEffects() {
		playFireSound();
		effectsController.doEffects();
	}

	private void playFireSound() {
		// create sound event
		//float volume = gunshotSoundEmitter.volume;

		// play sound effect
		if (gunshotSoundEmitter != null) {
		    //gunshotSoundEmitter.clip = fireSounds[UnityEngine.Random.Range(0, fireSounds.Length - 1)];
			gunshotSoundEmitter.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
		}
		playSound(gunshotSoundEmitter);
	}

	[ClientCallback]
	void OnGUI() {
		//GUI.Window(0, new Rect(GUI., 5, 50, 50), windowFunction, GUIContent.none);
		if (transform.parent != null && transform.parent.GetComponent<Player>().isLocalPlayer) {

			GUI.skin = guiSkin;

			int padding = 10;
			int boxWidth = 80 + padding * 3;
			int boxHeight = 30 + padding * 2;
			int boxCornerX = Screen.width - boxWidth - padding;
			int boxCornerY = Screen.height - boxHeight - padding;
			GUI.color = new Color(0, 0, 0, 0.6f);
			GUI.DrawTexture(new Rect(boxCornerX - 2, boxCornerY - 2, boxWidth + 4, boxHeight + 4), blackTexture);
			GUI.color = Color.white;
			GUI.Box(new Rect(boxCornerX, boxCornerY, boxWidth, boxHeight), GUIContent.none);

			//int imageWidth = 50;
			//int imageHeight = 50;
			//GUI.DrawTexture(new Rect(boxCornerX + padding, boxCornerY + padding * 2 + 20, imageWidth, imageHeight), bulletIcon);
			GUI.TextArea(new Rect(boxCornerX + padding * 2, boxCornerY + padding, 60, 40), "" + Mathf.Ceil((float)bulletsRemaining / (float)magazineSize));
			GUI.TextArea(new Rect(boxCornerX + padding * 2 + 40, boxCornerY + padding, 60, 40), "" + currentMagazineFill);
		}
	}
}
