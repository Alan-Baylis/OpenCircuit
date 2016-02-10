using UnityEngine;
using System.Collections;

public class AutoDoor : MonoBehaviour {

    public float doorHeight = 3.5f;
	public float toggleTime = .25f;

	public GameObject door;

    private Vector3 downPosition;
    private Vector3 upPosition;
    private bool isMovingUp = true;
	private bool atEnd = false;

	private AudioSource soundEmitter;

	public AudioClip endSound;

    // Use this for initialization
    void Start () {
		soundEmitter = gameObject.AddComponent<AudioSource>();
        downPosition = door.transform.position - new Vector3 (0,doorHeight,0);
        upPosition = door.transform.position;
	}

	public void open() {
		if(isMovingUp) {
			isMovingUp = false;
			atEnd = false;
		}
	}

	public void close() {
		if(!isMovingUp) {
			isMovingUp = true;
			atEnd = false;
		}
	}

	public void toggle() {
		isMovingUp = !isMovingUp;
		atEnd = false;
	}

    void moveDown() {
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

    void moveUp() {
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
	// Update is called once per frame
	void Update () {

    if (isMovingUp) {
            moveUp();
        }
     else {
            moveDown();
        }

	}
}