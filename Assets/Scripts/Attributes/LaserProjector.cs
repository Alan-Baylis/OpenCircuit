using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LaserProjector : MonoBehaviour {

	public float laserWidth = .01f;
	public int numLasers = 1;
	public float laserSpread = .5f;
	public float movementRange = .5f;
	public float laserLength = 1f;
	public float verticalOffset = .25f;
	public Color color = new Color(.298f, .27f, 1f, .5f);
	public float scanTime = 3f;
	public float zOffset = 0f;
	private List<LineRenderer> lineRenderers = new List<LineRenderer>();
	private List<GameObject> gameObjects = new List<GameObject>();
	private float upness=0;
	private bool movingUp = true;
	private bool active = false;

	private AudioSource soundEmitter;
	public AudioClip scanSound;
	private float currentScanTime = 0f;

	private RobotController currentController;

	void Start() {
		soundEmitter = gameObject.AddComponent<AudioSource>();
		soundEmitter.enabled = true;
		soundEmitter.loop = false;

		upness = -movementRange;

		for(int i = 0; i < numLasers; i++) {
			lineRenderers.Add(createRenderer());
		}
		upness = movementRange;
	}

	void Update() {
		if(isScanning()) {
			currentScanTime += Time.deltaTime;
		}

		if(currentScanTime > scanTime) {
			currentScanTime = 0;
			stopScan();
			if(currentController == null) {
				currentController = GetComponentInParent<RobotController>();
			}
			currentController.enqueueMessage(new RobotMessage("target scanned", null, new Vector3(), null));
		}


		if(active) {
			if(!soundEmitter.isPlaying) {
			//soundEmitter.clip = scanSound;
				soundEmitter.PlayOneShot(scanSound);
			}
			float movementRate = 4f*movementRange/(scanTime);
			drawLines();
			if(movingUp) {
				upness += Time.deltaTime * movementRate;
				if(upness > movementRange) {
					movingUp = false;
				}
			} else {
				upness -= Time.deltaTime * movementRate;
				if(Mathf.Abs(upness) > movementRate) {
					movingUp = true;
				}
			}
		} else {
			if(soundEmitter.isPlaying) {
				soundEmitter.Stop();
			}
			clearLines();
		}
	}

	public void setController(RobotController controller) {
		currentController = controller;
	}

	public void startScan() {
		active = true;
	}

	public void stopScan() {
		active = false;
	}

	public bool isScanning() {
		return active;
	}

	private void drawLines() {
		Vector3 line = transform.position + (transform.forward * laserLength);
		float width = laserSpread;
		for(int i = 0; i < lineRenderers.Count; i++) {
			LineRenderer renderer = lineRenderers[i];
			renderer.positionCount = 2;
			float rightness = -(width / 2f) + ((float)i) * (width / numLasers);
			renderer.SetPosition(0, transform.position+new Vector3(0f, 0f, zOffset));

			renderer.SetPosition(1, line + transform.right * rightness + transform.up*upness + transform.up*verticalOffset);
		}
	}

	private void clearLines() {
		foreach(LineRenderer lineRenderer in lineRenderers) {
			lineRenderer.positionCount = 0;
		}
	}

	private LineRenderer createRenderer() {
		GameObject temp = new GameObject("LaserLine");
		gameObjects.Add(temp);
		LineRenderer lineRenderer = temp.AddComponent<LineRenderer>();
		lineRenderer.startWidth = laserWidth;
		lineRenderer.startColor = color;
		lineRenderer.positionCount = 2;
		lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
		lineRenderer.material.color = color;
		return lineRenderer;
	}
}