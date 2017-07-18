using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

[AddComponentMenu("Scripts/Player/Player")]
public class Player : NetworkBehaviour {
	
	public bool drawReticle;
	public Texture2D reticle;
	public float maxOxygen = 60;
	public float oxygenRecoveryRate = 2;
	public float oxygen = 60;
//	public Texture2D sufferingOverlay;
	public SufferingErrorConfig sufferingErrorConfig;
	public AudioClip heavyBreathingSound;
	public AudioClip teleportSound;
	public float whiteOutDuration;
	public float blackOutDuration;
	public EffectSpec destroyEffect;
    [SyncVar]
    public bool frozen;

	[HideInInspector]
	public bool zooming;

	private Attack myAttacker;
	private Grab myGrabber;
	private Interact myInteractor;
	private Inventory myInventory;
	private MovementController myMover;
	private Camera myCam;
	private CameraSync myHead;
	private MouseLook myLooker;
	private Controls myControls;
	private Health myHealth;

	private AudioSource breathingSource;
	private float whiteOutTime;
	private float blackOutTime = 0;
	private Texture2D whiteOutTexture;

	private HashSet<SufferingError> sufferingErrors = new HashSet<SufferingError>();
	private readonly string[] sufferingErrorMessages = {
		"Connection Lost",
		"Component Disabled",
		"Rebooting Component",
		"Response Out of Range",
		"Power Surge"
	};

	private bool alive = true;

	public EffectSpec effectSpec;

	[SyncVar]
	public NetworkInstanceId clientControllerId = NetworkInstanceId.Invalid;
    private ClientController myClientController;

	[SyncVar(hook = "changeEyeColor")]
	private Color eyeColor;
	
	public Attack attacker { get {
		if(myAttacker == null) {
			myAttacker = GetComponent<Attack>();
		}
		return myAttacker; 
	} set { myAttacker = value; } }

	public Grab grabber { get {
		if(myGrabber == null) {
			myGrabber = GetComponent<Grab>();
		}
		return myGrabber; } set { myGrabber = value; } }
	public Interact interactor { get {
		if(myInteractor == null) {
			myInteractor = GetComponent<Interact>();
		}
		return myInteractor; } set { myInteractor = value; } }
	public Inventory inventory { get {
		if(myInventory == null) {
			myInventory = GetComponent<Inventory>();
		}
		return myInventory; } set { myInventory = value; } }
	public MovementController mover { get {
		if(myMover == null) {
			myMover = GetComponent<MovementController>();
		}
		return myMover; } set { myMover = value; } }
	public Camera cam { get {
		if(myCam == null) {
			myCam = GetComponentInChildren<Camera>();
		}
		return myCam; 
		} set { myCam = value; } }
	public CameraSync head { get {
			if(myHead == null) {
				myHead = GetComponentInChildren<CameraSync>();
			}
			return myHead; 
		}}
	public MouseLook looker { get {
		if(myLooker == null) {
			myLooker = GetComponent<MouseLook>();
		}
		return myLooker; } set { myLooker = value; } }
	public Controls controls { get {
		if(myControls == null) {
			myControls = GetComponent<Controls>();
		}
		return myControls; } set { myControls = value; } }
	public Health health { get {
		if(myHealth == null) {
			myHealth = GetComponent<Health>();
		}
		return myHealth; } set { myHealth = value; } }


    public ClientController clientController {
        get {
	        if (myClientController == null && clientControllerId != NetworkInstanceId.Invalid)
		        myClientController = ClientScene.FindLocalObject(clientControllerId).GetComponent<ClientController>();
            return myClientController;
        }
    }

	void Awake() {		
		whiteOutTime = 0;
		breathingSource = gameObject.AddComponent<AudioSource>();
		breathingSource.clip = heavyBreathingSound;
		breathingSource.enabled = true;
		breathingSource.loop = true;
		whiteOutTexture = new Texture2D (1, 1, TextureFormat.RGB24, false);
		whiteOutTexture.SetPixel (0, 0, Color.white);
		fadeIn(); // fade in for dramatic start
	}


	public void Start() {
		if (isServer) {
			TeamId team = GetComponent<TeamId>();
			if (team.enabled) {
				eyeColor = team.team.config.color;
			} else {
				eyeColor = Color.blue;
			}
			GetComponent<ScoreAgent>().owner = clientController;
		}
		//Allow the player to exist without a client controller
		if (clientController != null) {
			clientController.setPlayer(gameObject);
		}
	}

    [ClientCallback]
	void Update () {
        if (isLocalPlayer) {
            if (oxygen < maxOxygen - oxygenRecoveryRate * Time.deltaTime) {
                oxygen += oxygenRecoveryRate * Time.deltaTime;
                if (!breathingSource.isPlaying) {
                    breathingSource.Play();
                }
                breathingSource.volume = Mathf.Max(0, 1 - oxygen / maxOxygen);
            } else {
                oxygen = maxOxygen;
                if (breathingSource.isPlaying) {
                    breathingSource.Stop();
                }
            }
        }
	}

	public void physicsPickup(GameObject item) {

	}

	public void lockMovement() {
		mover.lockMovement();
	}

	public void fadeIn() {
		blackOutTime = blackOutDuration;
	}

	public void blackout(float seconds) {
		blackOutTime = Mathf.Max(seconds, blackOutTime);
	}

	[Server]
	public void die() {
		if (!alive)
			return;
	    GlobalConfig.globalConfig.gamemode.onPlayerDeath(this);
	}

	[Server]
	public void dismantle() {
		enabled = false;
		RpcDismantle();
	}

	[ClientRpc]
	public void RpcDismantle() {
		enabled = false;
		dismantleEffect();
	}

	public void dismantleEffect() {
		destroyEffect.spawn(transform.position);
		while (transform.childCount > 0) {
			DismantleEffect.dismantle(transform.GetChild(0), 30, isServer, GetComponent<Rigidbody>().velocity);
		}
		Destroy(gameObject);
	}

	public void teleport(Vector3 position) {
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		breathingSource.PlayOneShot(teleportSound);
		transform.position = position;
		whiteOutTime = whiteOutDuration;
	}

	private void changeEyeColor(Color color) {
		eyeColor = color;
		Renderer renderer = head.transform.Find("Eye").GetComponent<Renderer>();
		if (renderer != null) {
			Material mat = renderer.material;

			mat.SetColor("_EmissionColor", eyeColor);
			mat.SetColor("_Albedo", eyeColor);
		}
	}

	[ClientCallback]
	public void OnGUI () {
		if (!isLocalPlayer)
			return;

		// draw the reticle
		if (drawReticle) {
			Rect reticleShape = new Rect(Screen.width/2 -10, Screen.height/2 -8, 16f,16f);
			GUI.DrawTexture(reticleShape, reticle);
		}

		// draw the "coldness" screen shader

		if (whiteOutTime > 0) {
			GUI.color = new Color(1, 1, 1, whiteOutTime /whiteOutDuration *2);
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), whiteOutTexture);
			GUI.color = Color.white;
			whiteOutTime -= Time.deltaTime;
			if (whiteOutTime < 0)
				whiteOutTime = 0;
		}

//		if (health.getDamage() > 0) {
//			GUI.color = new Color(1, 0.2f, 0.2f, myHealth.getDamagePercent() * 0.5f);
//			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), sufferingOverlay);
//			GUI.color = Color.white;
//		}

		if (blackOutTime > 0) {
			GUI.color = new Color(0, 0, 0, blackOutTime / blackOutDuration);
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), whiteOutTexture);
			GUI.color = Color.white;
			blackOutTime -= Time.deltaTime;
			if (blackOutTime < 0)
				blackOutTime = 0;
		}
		
		if (Time.timeScale == 0) {
			GUI.color = Color.black;
			GUIStyle style = new GUIStyle(GUI.skin.label);
			style.fontSize = 30;
			style.alignment = TextAnchor.MiddleCenter;
			GUI.Label(new Rect(Screen.width /2 -100, Screen.height /2 -15, 200, 30),
				"Paused", style);
			GUI.color = Color.white;
		}

		// show suffering error messages
		if (sufferingErrors.Count < sufferingErrorConfig.maxErrorCount *health.getDamagePercent()) {
			Vector2 position = UnityEngine.Random.insideUnitCircle *0.5f;
			Vector2 positionSign = new Vector2(Mathf.Sign(position.x), Mathf.Sign(position.y)) *0.5f;
			position = new Vector2((positionSign.x *0.9f - position.x *0.5f *health.getDamagePercent()) *Screen.width /Screen.height,
				position.y *(health.getDamagePercent() +0.2f) + positionSign.y *(0.8f - health.getDamagePercent()));
			sufferingErrors.Add(new SufferingError {
				message = sufferingErrorMessages[UnityEngine.Random.Range(0, sufferingErrorMessages.Length)],
				position = position,
				fadeTimestamp = Time.time + sufferingErrorConfig.lingerTime
			});
		}
		GUIStyle errorStyle = new GUIStyle(GUI.skin.label);
		errorStyle.font = sufferingErrorConfig.font;
		errorStyle.fontSize = GUIUtil.adjustFontSize(sufferingErrorConfig.fontSize);
		errorStyle.alignment = TextAnchor.MiddleCenter;
		foreach(SufferingError error in sufferingErrors.ToList()) {
			if (error.resolved) {
				error.opacity -= sufferingErrorConfig.fadeOutRate *sufferingErrorConfig.opacity *Time.deltaTime;
				if (error.opacity <= 0)
					sufferingErrors.Remove(error);
			} else if (error.opacity < sufferingErrorConfig.opacity) {
				error.opacity = Mathf.Min(sufferingErrorConfig.opacity,
					error.opacity + sufferingErrorConfig.fadeInRate *sufferingErrorConfig.opacity *Time.deltaTime);
			} else if (error.fadeTimestamp <= Time.time) {
				error.resolved = true;
			}

			GUI.color = new Color(1, 0, 0, error.opacity);
			GUI.Label(GUIUtil.convertRect(centeredRect(error.position + new Vector2(Screen.width /2f / Screen.height, 0.5f), Vector2.one)), error.message, errorStyle);
		}
		GUI.color = Color.white;

		//// draw the player's oxygen level
		//GUI.Label(new Rect(10, 30, 100, 20), oxygen.ToString());
	}
	
	private Rect centeredRect(Vector2 position, Vector2 size) {
		return new Rect(position - size / 2, size);
	}

	private class SufferingError {
		public String message;
		public Vector2 position;
		public float opacity;
		public float fadeTimestamp;
		public bool resolved;
	}

    [Serializable]
	public struct SufferingErrorConfig {
		public Font font;
		public float fontSize, opacity, fadeInRate, fadeOutRate, lingerTime;
	    public int maxErrorCount;
    }
}
