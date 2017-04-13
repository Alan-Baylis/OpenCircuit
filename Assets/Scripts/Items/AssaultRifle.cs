using UnityEngine;
using UnityEngine.Networking;

public class AssaultRifle : AbstractGun {

    public AudioClip[] fireSounds;

	public float soundExpirationTime = 10f;
	public float inaccuracy = 0.1f;
	public float range = 1000;
	public float damage = 10;
	public float impulse = 1;

	public EffectSpec hitEffect;
	public EffectSpec robotHitEffect;

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
			Health health = getParentComponent<Health>(hitInfo.collider.transform);
			if (health != null) {
				CmdBulletHitHealth(direction, hitInfo.point, hitInfo.normal, health.netId);

				Rigidbody rb = health.GetComponent<Rigidbody>();
				if (rb != null) {
					rb.AddForceAtPosition(direction * impulse, hitInfo.point);
				}

				// do ricochet
				//if (-Vector3.Dot(direction, hitInfo.normal) < 0.5f) {
				//	doBullet(hitInfo.point, Vector3.Reflect(direction, hitInfo.normal), power -0.25f);
				//}
				robotHitEffect.spawn(hitInfo.point, hitInfo.normal);
			} else {
				CmdBulletHit(direction, hitInfo.point, hitInfo.normal);
				hitEffect.spawn(hitInfo.point, hitInfo.normal);
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
		Health health = hitObject.GetComponent<Health>();
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
		if(health != null) {
			health.hurt(calculateDamage(direction, normal));
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
	protected override void RpcCreateShotEffect(HitEffectType type, Vector3 location, Vector3 normal) {
		if (!hasAuthority) {
			if (type == HitEffectType.DEFAULT) {
				hitEffect.spawn(location, normal);
			} else if (type == HitEffectType.ROBOT) {
				robotHitEffect.spawn(location, normal);
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

		// do fire effects
		Vector3 effectPosition = transform.TransformPoint(fireEffectLocation);
		fireEffect.spawn(effectPosition, -transform.forward);
		fireEffectSideways.spawn(effectPosition, -transform.right - transform.forward);
		fireEffectSideways.spawn(effectPosition, transform.right - transform.forward);
		fireEffectLight.spawn(effectPosition);
	}

	private void playFireSound() {
		// create sound event
		//float volume = gunshotSoundEmitter.volume;
		if (Time.time - lastFiredTime > .5f || audioLabel == null) {
			audioLabel = new LabelHandle(transform.position, "gunshots");
			audioLabel.addTag(new SoundTag(TagEnum.Sound, 0, audioLabel, Time.time, soundExpirationTime));
			audioLabel.addTag(new Tag(TagEnum.Threat, 0, audioLabel));

			audioLabel.setPosition(transform.position);
			Tag soundTag = audioLabel.getTag(TagEnum.Sound);
			Tag threatTag = audioLabel.getTag(TagEnum.Threat);
			//soundTag.severity += (volume * 2 - soundTag.severity) * fireSoundThreatRate;
			//threatTag.severity += (fireSoundThreatLevel - threatTag.severity) * fireSoundThreatRate;
			AudioBroadcaster.broadcast(audioLabel, gunshotSoundEmitter.volume);
		} else {
			audioLabel.setPosition(transform.position);
			//Tag soundTag = audioLabel.getTag(TagEnum.Sound);
			//Tag threatTag = audioLabel.getTag(TagEnum.Threat);
			//soundTag.severity += (volume * 2 - soundTag.severity) * fireSoundThreatRate;
			//threatTag.severity += (fireSoundThreatLevel - threatTag.severity) * fireSoundThreatRate;
		}
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
