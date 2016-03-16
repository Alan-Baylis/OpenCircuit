using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public abstract class Item : NetworkBehaviour {

    public Texture2D icon;
	public Vector3 holdPosition;
	public Vector3 holdRotation;
	public Vector3 sprintPosition;
	public Vector3 sprintRotation;
	public float responsiveness = 0.5f;
	public float positionalResponsiveness = 0.5f;
	public float maxResponsiveness = 0.5f;
	public Vector3 stepBump = new Vector3(0, -0.1f, 0.05f);

	protected float reachDistance = 3f;

	protected Inventory holder;
	protected Collider col;
	protected Transform followingCamera;
	protected bool rightStepNext;

	public void Awake() {
		col = GetComponent<Collider>();
	}

	public virtual void Update() {
		if (followingCamera != null) {

			// determine desired position
			Vector3 newPosition;
			Quaternion newRotation;
            if (holder.sprinting) {
				Quaternion rotation = Quaternion.Euler(0, followingCamera.localEulerAngles.y, 0);
				newPosition = transform.parent.TransformPoint(rotation *sprintPosition);
				newRotation = transform.parent.rotation * Quaternion.Euler(sprintRotation);
				newRotation = rotation * newRotation;
			} else {
				newPosition = followingCamera.TransformPoint(holdPosition);
				newRotation = followingCamera.rotation * Quaternion.Euler(holdRotation);
			}

			// track to desired position
			transform.position += Vector3.ClampMagnitude((newPosition -transform.position) *positionalResponsiveness, maxResponsiveness);
			transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, responsiveness);
		}
	}

	public virtual void doStep(float strength) {
		Vector3 nextStepBump = stepBump *strength;
		if (!rightStepNext)
			nextStepBump.x = -nextStepBump.x;
		transform.position += transform.TransformVector(nextStepBump);
		rightStepNext = !rightStepNext;
	}

	public abstract void beginInvoke(Inventory invoker);

	public virtual void endInvoke(Inventory invoker) {}

    public virtual void onTake(Inventory taker) {
        transform.SetParent(taker.transform);
		followingCamera = taker.getPlayer().cam.transform;
		holder = taker;
        gameObject.SetActive(false);
    }

    public virtual void onDrop(Inventory taker) {
        transform.SetParent(null);
		followingCamera = null;
		gameObject.SetActive(true);
		endInvoke(taker);
    }

    public virtual void onEquip(Inventory equipper) {
		transform.localPosition = holdPosition;
		col.enabled = false;
		gameObject.SetActive(true);
    }

    public virtual void onUnequip(Inventory equipper) {
		gameObject.SetActive(false);
		endInvoke(equipper);
		col.enabled = true;
	}

	protected RaycastHit reach() { Vector3 fake; return reach(out fake); }
	protected RaycastHit reach(out Vector3 position) {
		RaycastHit finalHit = new RaycastHit();
		position = Vector3.zero;
		Ray ray = new Ray (this.transform.parent.position, this.transform.parent.forward);
		RaycastHit[] hits = Physics.RaycastAll(ray, reachDistance);
		float distance = reachDistance;

		foreach(RaycastHit hit in hits) {
			if (hit.distance > distance) continue;
			if (hit.collider.isTrigger) continue;
			finalHit = hit;
			distance = hit.distance;
			position = hit.point;
		}
		return finalHit;
	}
}
