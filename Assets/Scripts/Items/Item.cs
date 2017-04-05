using UnityEngine;
using UnityEngine.Networking;

public abstract class Item : NetworkBehaviour {

	public Texture2D icon;
	public HoldPosition normalPosition;
	public HoldPosition sprintPosition;
	public HoldPosition zoomPosition;
	public Vector3 grabPosition;
	public Vector3 secondaryGrabPosition;
	public float responsiveness = 0.5f;
	public float positionalResponsiveness = 0.5f;
	public float maxResponsiveness = 0.5f;
	public Vector3 stepBump = new Vector3(0, -0.1f, 0.05f);
	public float zoomLevel = 1.5f;

	protected float reachDistance = 3f;

	protected Inventory holder;
	protected Collider col;
	private Transform myFollowingCamera;

	protected Transform followingCamera
	{
		get
		{
			if (holder != null && myFollowingCamera == null) {
				myFollowingCamera = holder.getPlayer().cam.transform;
			}
			return myFollowingCamera;
		}
		set { myFollowingCamera = value; }
	}
	protected bool rightStepNext;

	[SyncVar]
	private NetworkInstanceId parent;

	public void Awake() {
		col = GetComponent<Collider>();
	}

	public override void OnStartClient() {
		base.OnStartClient();
		if (parent != null) {
			GameObject parentObj = ClientScene.FindLocalObject(this.parent);
			if (parentObj != null) {
				Inventory inventory = parentObj.GetComponent<Inventory>();
				if (inventory != null) {
					inventory.take(this);
				}
			}
		}
	}

	public void onTake(NetworkInstanceId parent) {
		this.parent = parent;
		transform.parent = ClientScene.FindLocalObject(this.parent).transform;
	}

	public virtual void Update() {
		if (followingCamera != null) {
			// reset desired zoom
			holder.getPlayer().looker.resetCameraZoom();

			// track to desired position
			float multiplier = holder.getPlayer().zooming ? 2f : 1.3f;
			moveTowardsPosition(getHoldPosition(), multiplier);
		}
	}

	public void setParent(NetworkInstanceId parent) {
		this.parent = parent;
	}

	public virtual void doStep(float strength) {
		Vector3 nextStepBump = stepBump * strength;
		if (!rightStepNext)
			nextStepBump.x = -nextStepBump.x;
		transform.position += transform.TransformVector(nextStepBump);
		rightStepNext = !rightStepNext;
	}

	public virtual HoldPosition getHoldPosition() {
		if (holder.sprinting) {
			return sprintPosition;
		} else if (holder.getPlayer().zooming) {
			holder.getPlayer().looker.setCameraZoom(zoomLevel);
			return zoomPosition;
		} else {
			return normalPosition;
		}
	}

	public abstract void beginInvoke(Inventory invoker);

	public virtual void endInvoke(Inventory invoker) { }

	public virtual void onTake(Inventory taker) {
		transform.SetParent(taker.transform);
		holder = taker;
		gameObject.SetActive(false);
	}

	public virtual void onDrop(Inventory taker) {
		transform.SetParent(null);
		myFollowingCamera = null;
		gameObject.SetActive(true);
		endInvoke(taker);
		holder = null;
	}

	public virtual void onEquip(Inventory equipper) {
		transform.localPosition = normalPosition.position;
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
		Ray ray = new Ray(this.transform.parent.position, this.transform.parent.forward);
		RaycastHit[] hits = Physics.RaycastAll(ray, reachDistance);
		float distance = reachDistance;

		foreach (RaycastHit hit in hits) {
			if (hit.distance > distance) continue;
			if (hit.collider.isTrigger) continue;
			finalHit = hit;
			distance = hit.distance;
			position = hit.point;
		}
		return finalHit;
	}

	protected void moveTowardsPosition(HoldPosition position, float responsivenessMultiplier) {
		Vector3 newPosition;
		Quaternion newRotation;
		if (position.cameraRelative) {
			newPosition = followingCamera.TransformPoint(position.position);
			newRotation = followingCamera.rotation * Quaternion.Euler(position.rotation);
		} else {
			Quaternion rotation = Quaternion.Euler(0, holder.getPlayer().head.transform.localEulerAngles.y, 0);
			newPosition = transform.parent.TransformPoint(rotation * position.position);
			newRotation = transform.parent.rotation * Quaternion.Euler(position.rotation);
			newRotation = rotation * newRotation;
		}

		// track to desired position
		float posResponse = Mathf.Min(positionalResponsiveness * Time.deltaTime * responsivenessMultiplier, 1);
		float rotResponse = Mathf.Min(responsiveness * Time.deltaTime * responsivenessMultiplier, 1);
		transform.position += (newPosition - transform.position) * posResponse;
		transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, rotResponse);
	}

	[System.Serializable]
	public struct HoldPosition {
		public Vector3 position;
		public Vector3 rotation;
		public bool cameraRelative;
	}
}
