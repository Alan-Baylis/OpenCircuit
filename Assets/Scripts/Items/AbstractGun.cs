﻿using UnityEngine;

using UnityEngine.Networking;

public abstract class AbstractGun : Item {

	public float soundExpirationTime = 10f;

	public float fireSoundThreatLevel = 5;
	public float fireSoundThreatRate = 0.3f;
	public float fireSoundVolume = 1;
	public float fireDelay = 0.1f;

	public float baseInaccuracy = 0.1f;
	public float focusInnaccuracy = 0.5f;
	public float maximumMovementInaccuracy = 0.1f;
	public float movementInaccuracySoftness = 10f;
	public Vector3 recoilAnimationDistance = new Vector3(0, 0, 0.2f);
	public Vector3 focusRecoilAnimationDistance = new Vector3(0, 0, 0.2f);
	public Vector2 recoilMinRotation = new Vector3(-0.1f, 0.1f);
	public Vector2 recoilMaxRotation = new Vector3(0.1f, 0.2f);
	public HoldPosition reloadPosition;

	public GunEffectsController effectsController;

	public GUISkin guiSkin;
		
	protected bool shooting = false;
	protected bool reloading = false;

	protected float lastFiredTime = 0;
	protected float reloadTimeRemaining = 0;

	public AudioSource gunshotSoundEmitter;
	public AudioSource reloadSoundEmitter;

	[ClientCallback]
	public override void Update() {
		base.Update();
		if (lastFiredTime <= Time.time - fireDelay && shooting && !reloading) {
			Transform cam = holder.getPlayer().cam.transform;
			shoot(cam.position, cam.forward);

		} else if(reloading) {
			reloadTimeRemaining -= Time.deltaTime;
			if(reloadTimeRemaining <= 0) {
				reloading = false;
			}
		}
	}

	public override HoldPosition getHoldPosition() {
		if (reloading)
			return reloadPosition;
		return base.getHoldPosition();
	}

	public override void beginInvoke(Inventory invoker) {
		shooting = true;
	}

	public override void endInvoke(Inventory invoker) {
		shooting = false;
	}

	public abstract void reload();

	protected abstract bool isLoaded();

	protected abstract void doFireEffects();

	protected abstract void consumeAmmo();

	public abstract bool addAmmo(int quantity);

	[ClientRpc]
	protected abstract void RpcCreateShotEffect(HitEffectType type, Vector3 location, Vector3 direction, Vector3 normal);

	[ClientRpc]
	protected abstract void RpcCreateFireEffects();

	[Server]
	protected abstract void applyDamage(NetworkInstanceId hit, Vector3 direction, Vector3 normal);

	[Client]
	protected void shoot(Vector3 position, Vector3 direction) {
		if(isLoaded()) {
			if (!isServer) {
				consumeAmmo();
			}
			direction = inaccurateDirection(direction, getMovementInaccuracy());
            direction = inaccurateDirection(direction, holder.getPlayer().zooming ? focusInnaccuracy : baseInaccuracy);
            doBullet(position, direction, 1);
			lastFiredTime = Time.time;
			transform.position -= transform.TransformVector(holder.getPlayer().zooming ? focusRecoilAnimationDistance : recoilAnimationDistance);

			doFireEffects();
			MouseLook looker = holder.getPlayer().looker;
			looker.rotate(
				Random.Range(recoilMinRotation.x, recoilMaxRotation.x),
				Random.Range(recoilMinRotation.y, recoilMaxRotation.y));
		} else { 
			playReloadSound();
			reload();
		}
	}

	protected virtual float getMovementInaccuracy() {
		// here we use a rational function to get the desired behaviour
		const float arbitraryValue = 0.2f; // the larger this value is, the faster the player must be moving before it affects his accuracy
		float speed = holder.GetComponent<Rigidbody>().velocity.magnitude;
		float inaccuracy = (maximumMovementInaccuracy * speed -arbitraryValue) / (speed +movementInaccuracySoftness);
		return Mathf.Max(inaccuracy, 0);
	}

	[Client]
	protected abstract void doBullet(Vector3 position, Vector3 direction, float power);

	[Command]
	protected virtual void CmdBulletHitLabel(Vector3 direction, Vector3 position, Vector3 normal, NetworkInstanceId hit) {
		serverDoBullet(direction, position, normal, hit);
		RpcCreateShotEffect(HitEffectType.ROBOT, position, direction, normal);
	}

	[Command]
	protected virtual void CmdBulletHit(Vector3 direction, Vector3 position, Vector3 normal) {
		serverDoBullet(direction, position, normal);
		RpcCreateShotEffect(HitEffectType.DEFAULT, position, direction, normal);
	}

	[Command]
	protected virtual void CmdBulletMiss(Vector3 direction) {
		serverDoBullet(direction);
		RpcCreateFireEffects();
	}

	[Server]
	private void serverDoBullet(Vector3 direction, Vector3? position = null, Vector3? normal = null, NetworkInstanceId? hit = null) {
		consumeAmmo();
		if (hit != null) {
			applyDamage(hit.Value, direction, normal.Value);
		}
		if (Time.time - lastFiredTime > 1f) {
			LabelHandle audioLabel = new LabelHandle(transform.position, "gunshots");
			TeamId team = holder.GetComponent<TeamId>();
			if (team != null && team.enabled) {
				audioLabel.teamId = team.id;
			}
			audioLabel.addTag(new SoundTag(TagEnum.Sound, 0, audioLabel, Time.time, soundExpirationTime));
			audioLabel.addTag(new Tag(TagEnum.Threat, 0, audioLabel));

			audioLabel.setPosition(transform.position);
			AudioBroadcaster.broadcast(audioLabel, gunshotSoundEmitter.volume);
		}
		if (!hasAuthority) {
			lastFiredTime = Time.time;
		}
	}


	public static Vector3 inaccurateDirection(Vector3 direction, float inaccuracy) {
		Vector3 randomAngle = Random.onUnitSphere;
		float angle = Vector3.Angle(direction, randomAngle) /360;
		return Vector3.RotateTowards(direction, Random.onUnitSphere, Mathf.PI *angle *inaccuracy, 0);
	}

	protected void playSound(AudioSource soundEmitter) {
		if(soundEmitter != null && soundEmitter.clip != null) {
			soundEmitter.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
			soundEmitter.Play();
		} else if(soundEmitter == null) {
			Debug.LogWarning("AudioSource not set for the '"+GetType()+"' component attached to '" + gameObject.name + "'");
		} else if (soundEmitter.clip == null) {
			Debug.LogWarning("AudioSource clip missing for the '" + GetType() + "' component attached to '" + gameObject.name + "'");
		}
	}

	private void playReloadSound() {
		if (reloadSoundEmitter != null) {
			reloadSoundEmitter.volume = fireSoundVolume;
		}
		playSound(reloadSoundEmitter);
	}
}
