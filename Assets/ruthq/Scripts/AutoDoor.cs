using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class AutoDoor : NetworkBehaviour {

	public List<Label> doorLocks;

    public float doorHeight = 3.5f;
	public float toggleTime = .25f;

	public GameObject door;

	public AudioClip endSound;

	private List<Label> activeDoorLocks = new List<Label>();

    private Vector3 downPosition;
    private Vector3 upPosition;
    private bool isMovingUp = true;
	private bool atEnd = false;

	private AudioSource soundEmitter;


    // Use this for initialization
	[ServerCallback]
    void Start () {
		if(doorLocks != null && doorLocks.Count > 0) {
			foreach (Label lockLabel in doorLocks) {
				if (lockLabel != null) {
					DoorControlDestruction operation = new DoorControlDestruction();
					operation.setDoor(this);
					lockLabel.addOperation(operation, new System.Type [] {typeof(DestructTrigger)});
					activeDoorLocks.Add(lockLabel);
				}
			}
		} 
		soundEmitter = gameObject.AddComponent<AudioSource>();
        downPosition = door.transform.position - new Vector3 (0,doorHeight,0);
        upPosition = door.transform.position;
	}

	// Update is called once per frame
	[ServerCallback]
	void Update() {

		if(isMovingUp) {
			//print("moving up");
			moveUp();
		} else {
			moveDown();
		}

	}

	[Server]
	public void removeDoorLock(Label doorLock) {
		activeDoorLocks.Remove(doorLock);
		if(activeDoorLocks.Count < 1) {
			open();
		}
	}

	[Server]
	public void open() {
		if(isMovingUp) {
			isMovingUp = false;
			atEnd = false;
		}
	}

	[Server]
	public void close() {
		if(!isMovingUp) {
			isMovingUp = true;
			atEnd = false;
		}
	}


	[Server]
	public void toggle() {
		isMovingUp = !isMovingUp;
		atEnd = false;
	}

	[Server]
   private void moveDown() {
        Vector3 stopVector = door.transform.position - downPosition;
        float length = stopVector.magnitude;

		float distanceToMove = Time.deltaTime * doorHeight / toggleTime;

        if (length > distanceToMove){ 
			door.transform.position = door.transform.position - new Vector3(0, distanceToMove, 0);
		} else {
			if(endSound != null && !atEnd) {
				soundEmitter.PlayOneShot(endSound);
				atEnd = true;
			}
			door.transform.position = downPosition;
		}

    }

	[Server]
    private void moveUp() {
        Vector3 upVector = door.transform.position - upPosition;
        float upLength = upVector.magnitude;

		float distanceToMove = Time.deltaTime * doorHeight / toggleTime;

        if (upLength > distanceToMove) {
			door.transform.position = door.transform.position + new Vector3(0, distanceToMove, 0);
		} else {
			if(endSound != null && !atEnd) {
				soundEmitter.PlayOneShot(endSound);
				atEnd = true;
			}
			door.transform.position = upPosition;
		}

    }
}