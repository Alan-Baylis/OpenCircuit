﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[AddComponentMenu("Scripts/Player/Player")]
public class Player : NetworkBehaviour {
	
	public bool drawReticle;
	public Texture2D reticle;
	public float maxOxygen = 60;
	public float oxygenRecoveryRate = 2;
	public float oxygen = 60;
	public Texture2D sufferingOverlay;
	public AudioClip heavyBreathingSound;
	public AudioClip teleportSound;
	public float whiteOutDuration;
	public float blackOutDuration;
	public bool showFPSMeter = false;

	[HideInInspector]
	public ClientController controller;
	[HideInInspector]
	public bool zooming = false;
	
	private Attack myAttacker;
	private Grab myGrabber;
	private Interact myInteractor;
	private Inventory myInventory;
	private MovementController myMover;
	private Camera myCam;
	private MouseLook myLooker;
	private Controls myControls;
	private Health myHealth;

	private AudioSource breathingSource;
	private float whiteOutTime;
	private float blackOutTime = 0;
	private Texture2D whiteOutTexture;

	private bool alive = true;
	private float deltaTime;

	public EffectSpec effectSpec;
	
	public Attack attacker { get { return myAttacker; } set { myAttacker = value; } }
	public Grab grabber { get { return myGrabber; } set { myGrabber = value; } }
	public Interact interactor { get { return myInteractor; } set { myInteractor = value; } }
	public Inventory inventory { get { return myInventory; } set { myInventory = value; } }
	public MovementController mover { get { return myMover; } set { myMover = value; } }
	public Camera cam { get { return myCam; } set { myCam = value; } }
	public MouseLook looker { get { return myLooker; } set { myLooker = value; } }
	public Controls controls { get { return myControls; } set { myControls = value; } }
	public Health health { get { return myHealth; } set { myHealth = value; } }

	void Awake() {
		attacker = GetComponent<Attack> ();
		grabber = GetComponent<Grab>();
		interactor = GetComponent<Interact>();
		inventory = GetComponent<Inventory>();
		mover = GetComponent<MovementController>();
		cam = GetComponentInChildren<Camera>();
		looker = GetComponent<MouseLook>();
		controls = GetComponent<Controls>();
		myHealth = GetComponent<Health>();
		
		whiteOutTime = 0;
		breathingSource = gameObject.AddComponent<AudioSource>();
		breathingSource.clip = heavyBreathingSound;
		breathingSource.enabled = true;
		breathingSource.loop = true;
		whiteOutTexture = new Texture2D (1, 1, TextureFormat.RGB24, false);
		whiteOutTexture.SetPixel (0, 0, Color.white);
		fadeIn(); // fade in for dramatic start
	}

	void Update () {
		if (oxygen < maxOxygen - oxygenRecoveryRate *Time.deltaTime) {
			oxygen += oxygenRecoveryRate *Time.deltaTime;
			if (!breathingSource.isPlaying) {
				breathingSource.Play();
			}
			breathingSource.volume = Mathf.Max(0, 1 -oxygen /maxOxygen);
		} else {
			oxygen = maxOxygen;
			if (breathingSource.isPlaying) {
				breathingSource.Stop();
			}
		}

		deltaTime += (Time.deltaTime -deltaTime) *0.1f;
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
		//alive = false;
		//blackOutTime = blackOutDuration;
		//Menu.menu.lose();
		effectSpec.spawn(transform.position);
		if(controller != null) {
			controller.destroyPlayer(connectionToClient, playerControllerId);
		}
	}

	public void teleport(Vector3 position) {
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		breathingSource.PlayOneShot(teleportSound);
		transform.position = position;
		whiteOutTime = whiteOutDuration;
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

		if (myHealth.getDamage() > 0) {
			GUI.color = new Color(1, 0.2f, 0.2f, myHealth.getDamagePercent() * 0.5f);
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), sufferingOverlay);
			GUI.color = Color.white;
		}

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

		if (showFPSMeter) {
			float msec = deltaTime * 1000;
			float fps = 1f / deltaTime;
			string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
			GUI.Label(new Rect(0, 0, 100, 20), text);
		}

		//// draw the player's oxygen level
		//GUI.Label(new Rect(10, 30, 100, 20), oxygen.ToString());
	}
}
