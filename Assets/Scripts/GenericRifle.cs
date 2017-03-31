using UnityEngine;
using UnityEngine.Networking;

public class GenericRifle : NetworkBehaviour {

    public AudioClip[] fireSounds;

    public float maximumMovementInaccuracy = 0.1f;
    public float movementInaccuracySoftness = 10f;
    public float baseInaccuracy = 0.1f;
    public float range = 1000;
    public float impulse = 1;
    public float damage = 10;
    public float fireDelay = 0.1f;

    public Vector3 fireEffectLocation;
    public EffectSpec fireEffect;
    public EffectSpec fireEffectSideways;
    public EffectSpec fireEffectLight;

    public EffectSpec robotHitEffect;
    public EffectSpec hitEffect;

    public AudioSource gunshotSoundEmitter;

    public bool firing;

    protected LabelHandle audioLabel;

    protected float lastFiredTime = 0;

    [ServerCallback]
    void Update() {
        if (lastFiredTime <= Time.time - fireDelay && firing) {
            // Transform cam = holder.getPlayer().cam.transform;
            shoot(transform.TransformPoint(fireEffectLocation), -transform.forward);

            //TODO: implement recoil?
//	        looker.rotate(
//	            Random.Range(recoilMinRotation.x, recoilMaxRotation.x),
//	            Random.Range(recoilMinRotation.y, recoilMaxRotation.y));
        }
    }

    public bool targetInRange(Vector3 targetPosition) {
        Vector3 objectVector = targetPosition - transform.TransformPoint(fireEffectLocation);
        return (1-Vector3.Dot(-transform.forward.normalized, objectVector.normalized)) < .05f;
    }

    [Server]
    protected void shoot(Vector3 position, Vector3 direction) {
        direction = inaccurateDirection(direction, getMovementInaccuracy());
        direction = inaccurateDirection(direction, baseInaccuracy);
        doBullet(position, direction, 1);
        lastFiredTime = Time.time;
        //transform.position -= transform.TransformVector(recoilAnimationDistance);

        doFireEffects();

    }

    public static Vector3 inaccurateDirection(Vector3 direction, float inaccuracy) {
        Vector3 randomAngle = Random.onUnitSphere;
        float angle = Vector3.Angle(direction, randomAngle) /360;
        return Vector3.RotateTowards(direction, Random.onUnitSphere, Mathf.PI *angle *inaccuracy, 0);
    }

    [Server]
    private void doBullet(Vector3 direction, Vector3? position = null, Vector3? normal = null, NetworkInstanceId? hit = null) {
        if (hit != null) {
            applyDamage(hit.Value, direction, normal.Value);
        }
    }

    [Server]
    protected void doBullet(Vector3 position, Vector3 direction, float power) {
        if (power <= 0)
            return;
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(position, direction, out hitInfo, range);
        if (hit) {
            Health health = getParentComponent<Health>(hitInfo.collider.transform);
            if (health != null) {
                bulletHitHealth(direction, hitInfo.point, hitInfo.normal, health.netId);

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
                bulletHit(direction, hitInfo.point, hitInfo.normal);
                hitEffect.spawn(hitInfo.point, hitInfo.normal);
            }
        } else {
            bulletMiss(direction);
        }
    }

    protected virtual float getMovementInaccuracy() {
        // here we use a rational function to get the desired behaviour
        const float arbitraryValue = 0.2f; // the larger this value is, the faster the player must be moving before it affects his accuracy
        float speed = getParentComponent<Rigidbody>(transform).velocity.magnitude;
        float inaccuracy = (maximumMovementInaccuracy * speed -arbitraryValue) / (speed +movementInaccuracySoftness);
        return Mathf.Max(inaccuracy, 0);
    }

    protected T getParentComponent<T>(Transform trans) where T:UnityEngine.Object {
        T comp = trans.GetComponent<T>();
        if(comp != null)
            return comp;
        if(trans.parent != null)
            return getParentComponent<T>(trans.parent);
        return null;
    }

    [Server]
    protected virtual void bulletHitHealth(Vector3 direction, Vector3 position, Vector3 normal, NetworkInstanceId hit) {
        doBullet(direction, position, normal, hit);
        RpcCreateShotEffect(HitEffectType.ROBOT, position, normal);
    }

    [Server]
    protected virtual void bulletHit(Vector3 direction, Vector3 position, Vector3 normal) {
        doBullet(direction, position, normal);
        RpcCreateShotEffect(HitEffectType.DEFAULT, position, normal);
    }

    [Server]
    protected virtual void bulletMiss(Vector3 direction) {
        doBullet(direction);
        RpcCreateFireEffects();
    }

    [ClientRpc]
    protected void RpcCreateFireEffects() {
        doFireEffects();
    }

    [ClientRpc]
    protected void RpcCreateShotEffect(HitEffectType type, Vector3 location, Vector3 normal) {
        if (!hasAuthority) {
            if (type == HitEffectType.DEFAULT) {
                hitEffect.spawn(location, normal);
            } else if (type == HitEffectType.ROBOT) {
                robotHitEffect.spawn(location, normal);
            }
            doFireEffects();
        }
    }

    [Server]
    protected void applyDamage(NetworkInstanceId hit, Vector3 direction, Vector3 normal) {
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


    private void playFireSound() {
        // create sound event
        //float volume = gunshotSoundEmitter.volume;
        if (Time.time - lastFiredTime > .5f || audioLabel == null) {
            audioLabel = new LabelHandle(transform.position, "gunshots");
            audioLabel.addTag(new Tag(TagEnum.Sound, 0, audioLabel));
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
            gunshotSoundEmitter.clip = fireSounds[UnityEngine.Random.Range(0, fireSounds.Length - 1)];
            gunshotSoundEmitter.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
        }
        playSound(gunshotSoundEmitter);
    }

    protected void doFireEffects() {
        playFireSound();

        // do fire effects
        Vector3 effectPosition = transform.TransformPoint(fireEffectLocation);
        fireEffect.spawn(effectPosition, -transform.forward);
        fireEffectSideways.spawn(effectPosition, -transform.right - transform.forward);
        fireEffectSideways.spawn(effectPosition, transform.right - transform.forward);
        fireEffectLight.spawn(effectPosition);
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
}
