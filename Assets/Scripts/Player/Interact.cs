using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Scripts/Player/Interact")]
public class Interact : MonoBehaviour {

	public Material highlightMaterial;
	public float grabDistance = 2.5f;

	private Player myPlayer;
	private Camera playerCam;
	private GameObject target;

	// Use this for initialization
	void Start () {
		myPlayer = this.GetComponent<Player> ();
		playerCam = myPlayer.cam;
	}

	void Update() {
		updateTarget();
	}

	private void updateTarget() {
		GameObject trgt = getTarget();

		if(target == trgt) return;

		if(target != null) {
			unHighlight();
		}

		target = trgt;
		if(trgt != null) {
			highlight();
		}
	}

	private void highlight() {
		Renderer rend = target.GetComponent<Renderer>();
		if(rend == null) return;
		Material[] mats = rend.materials;
		Material[] newMats = new Material[mats.Length * 2];
		System.Array.Copy(mats, newMats, mats.Length);
		for(int i = mats.Length; i < newMats.Length; ++i) {
			newMats[i] = highlightMaterial;
		}
		rend.materials = newMats;
	}

	private void unHighlight() {
		Renderer rend = target.GetComponent<Renderer>();
		if(rend == null) return;
		Material[] mats = rend.materials;
		Material[] newMats = new Material[mats.Length / 2];
		System.Array.Copy(mats, newMats, newMats.Length);
		rend.materials = newMats;
	}

	public void interact() {
		Grab grabber = myPlayer.grabber;
		Inventory inventory = myPlayer.inventory;
		if (grabber.hasObject ()) {
			grabber.dropObject ();
		} else {
			Vector3 point;
			GameObject nearest = getTarget(out point);
			if (nearest != null && !inventory.take(nearest)) {
				Label trgt;
				bool canInteract = hasInteractable(nearest, out trgt);
				if(canInteract) {
					InteractTrigger trig = new InteractTrigger();
					trig.setPoint(point);
					trgt.sendTrigger(myPlayer.gameObject, trig);
				} else {
					grabber.grabObject(nearest, point);
				}
            }
		}
	}

	private bool canInteract(GameObject obj) {
		Grab grabber = myPlayer.grabber;
		if (grabber.hasObject())
			return false;

		if (obj.GetComponent<Player>() != null)
			return false;
		if (myPlayer.inventory.canTake(obj))
            return true;
		Label output;
		if (hasInteractable(obj, out output))
            return true;
		return grabber.canGrabObject(obj);
	}

	private bool hasInteractable(GameObject obj, out Label target) {
		Label item = obj.GetComponent<Label>();
		if(item != null) {
			target = item;
			return item.hasOperationType(typeof(InteractTrigger));
		}
		target = null;
		return false;
	}

	//private GameObject reach() {Vector3 fake; return reach(out fake); }
	//private GameObject reach(out Vector3 position) {
	//	GameObject finalHit = null;
	//	position = Vector3.zero;
	//	Ray ray = playerCam.ScreenPointToRay (new Vector3 (playerCam.pixelWidth / 2, playerCam.pixelHeight / 2));
	//	RaycastHit[] hits = Physics.RaycastAll(ray, grabDistance);
	//	float distance = grabDistance;

	//	foreach(RaycastHit hit in hits) {
	//		if ((hit.collider.ClosestPointOnBounds(transform.position) -transform.position).sqrMagnitude > grabDistance *grabDistance) continue;
	//		if (hit.distance > distance) continue;
	//		if (hit.collider.isTrigger) continue;
	//		finalHit = hit.collider.gameObject;
	//		distance = hit.distance;
	//		position = hit.point;
	//	}
	//	return finalHit;
	//}

	protected GameObject getTarget() {
		Vector3 ignore;
		return getTarget(out ignore);
	}
    protected GameObject getTarget(out Vector3 contactPoint) {
		Transform camTrans = playerCam.transform;
		System.Nullable<RaycastHit> hit = cast(camTrans.position, camTrans.forward, grabDistance);
		if (hit == null || !canInteract(hit.Value.collider.gameObject)) {
			GameObject target = getTargetObject(out contactPoint);
			return target;
		}
		contactPoint = hit.Value.point;
        return hit.Value.collider.gameObject;
	}

	protected GameObject getTargetObject(out Vector3 contactPoint) {
		List<GameObject> availables = getTargetObjects();
		GameObject selected = null;
		float min = new Vector2(Screen.width / 4, Screen.height / 4).magnitude;
		RaycastHit hit = new RaycastHit();
		hit.distance = grabDistance + 0.5f;
		hit.point = playerCam.transform.position + playerCam.transform.forward * hit.distance;
		RaycastHit[] hits = Physics.RaycastAll(playerCam.transform.position, playerCam.transform.forward, grabDistance + 0.5f);
		foreach (RaycastHit h in hits) {
			if (!h.collider.isTrigger && hit.distance > h.distance)
				hit = h;
		}
		foreach (GameObject point in availables) {
			Vector3 center = playerCam.WorldToScreenPoint(point.transform.position);
			Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
			Vector2 pointCenter = new Vector2(center.x, center.y);
			float dist = (screenCenter - pointCenter).magnitude;
			if (dist < min) {
				min = dist;
				selected = point;
			}
		}
		contactPoint = hit.point;
		return selected;
	}

	protected List<GameObject> getTargetObjects() {
		List<GameObject> availables = new List<GameObject>();
		Transform camTrans = playerCam.transform;
		Collider[] closeObjects = Physics.OverlapSphere(camTrans.position, grabDistance);

		foreach (Collider col in closeObjects) {
			if (col.gameObject == myPlayer.gameObject)
				continue;

			// filter out points that are behind the camera
			if (Vector3.Dot(camTrans.forward, col.transform.position - camTrans.position) < 0) continue;

			// filter out obstructed points
			System.Nullable<RaycastHit> hit = cast(camTrans.position, col.transform.position - camTrans.position, grabDistance);
			if (hit != null && hit.Value.collider.gameObject != col.gameObject) {
				continue;
			}

			if (!canInteract(col.gameObject))
				continue;

			availables.Add(col.gameObject);
		}
		return availables;
	}

	protected static System.Nullable<RaycastHit> cast(Vector3 position, Vector3 direction, float length) {
		RaycastHit[] hits = Physics.RaycastAll(position, direction, length);
		if (hits.Length < 1)
			return null;
		RaycastHit finalHit = hits[0];
		foreach (RaycastHit hit in hits) {
			if (hit.distance < finalHit.distance)
				finalHit = hit;
		}
		return finalHit;
	}
}
