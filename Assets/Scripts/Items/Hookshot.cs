using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Scripts/Items/Hookshot")]
public class Hookshot : Item {

	public float length = 20;
	public float reelSpeed = 10;
	public float acceleration = 2;

	protected Player player;
	protected Transform playerTransform;
	protected Rigidbody playerRigidbody;
	protected Vector3 attachPoint;
	protected bool targetReached;

    public override void beginInvoke(Inventory invoker) {
		if (reelingIn()) {
			breakConnection();
			return;
		}

		attachPoint = getTarget(invoker);
		if ((attachPoint -invoker.transform.position).sqrMagnitude < 1) {
			return;
		}
		player = invoker.getPlayer();
		player.mover.lockMovement();
		playerRigidbody = player.GetComponent<Rigidbody>();
		playerTransform = player.transform;
		playerRigidbody.useGravity = false;
		//Vector3 desiredVel = (attachPoint -player.transform.position).normalized *reelSpeed;
		//playerRigidbody.velocity = desiredVel;
		targetReached = false;

		invoker.StartCoroutine(this.reelIn());
    }

	public override void onUnequip(Inventory equipper) {
		base.onUnequip(equipper);
		breakConnection();
	}

	public bool reelingIn() {
		return player != null;
	}
	
	public void breakConnection() {
		if (!reelingIn())
			return;
		player.mover.unlockMovement();
		if (playerRigidbody != null)
			playerRigidbody.useGravity = true;
		StopAllCoroutines();
		player = null;
	}

	public bool canGrapple(Collider target) {
//		print(target.material.name);
		return target.material != null && target.material.name.ToLower().StartsWith("grapple");
	}

	protected IEnumerator reelIn() {
		while(true) {
			Vector3 desiredVel = attachPoint -player.transform.position;
			if (desiredVel.sqrMagnitude < 1) {
				//breakConnection();
				//print ("Too close " +desiredVel);
				targetReached = true;
				yield return new WaitForFixedUpdate();
			}
			desiredVel = desiredVel.normalized *reelSpeed;
//			if (Vector3.Dot(playerRigidbody.velocity, desiredVel) /Vector3.Dot(desiredVel, desiredVel) < 0.05f) {
//				breakConnection();
////				print ("Low velocity");
//				yield return null;
//			}

			playerRigidbody.velocity = lerpVector3(playerRigidbody.velocity, desiredVel, acceleration);

			yield return new WaitForFixedUpdate();
		}
	}

	protected Vector3 getTarget(Inventory invoker) {
		Transform camTrans = invoker.getPlayer().cam.transform;
        RaycastHit[] hits = cast(camTrans.position, camTrans.forward, length);
		if (hits.Length < 1 || !canGrapple(hits[0].collider)) {
			GameObject target = getTargetObject(invoker);
			if (target == null)
				return invoker.transform.position;
			print("Used grapple point point.");
			return target.transform.position;
		}
		return hits[0].point;
	}

	protected GameObject getTargetObject(Inventory invoker) {
		List<GameObject> availables = getTargetObjects(invoker);
		print("Available: " +availables.Count);
		GameObject selected = null;
		float min = new Vector2(Screen.width /4, Screen.height /4).magnitude;
		RaycastHit hit = new RaycastHit();
		hit.distance = length + 0.5f;
		hit.point = invoker.getPlayer().cam.transform.position + invoker.getPlayer().cam.transform.forward * hit.distance;
		RaycastHit[] hits = Physics.RaycastAll(invoker.getPlayer().cam.transform.position, invoker.getPlayer().cam.transform.forward, length + 0.5f);
		foreach (RaycastHit h in hits) {
			if (!h.collider.isTrigger && hit.distance > h.distance)
				hit = h;
		}
		foreach (GameObject point in availables) {
			Vector3 center = invoker.getPlayer().cam.WorldToScreenPoint(point.transform.position);
			Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
			Vector2 pointCenter = new Vector2(center.x, center.y);
			float dist = (screenCenter - pointCenter).magnitude;
			if (dist < min) {
				min = dist;
				selected = point;
			}
		}
		//hitSpot.transform.position = hit.point;
		return selected;
	}

	protected List<GameObject> getTargetObjects(Inventory invoker) {
		List<GameObject> availables = new List<GameObject>();
		GameObject[] grapplePoints = GameObject.FindGameObjectsWithTag("Grapple Point");
		Transform camTrans = invoker.getPlayer().cam.transform;

		foreach (GameObject point in grapplePoints) {
			float distanceSqr = (point.transform.position - camTrans.position).sqrMagnitude;

			//filter out objects that are too far away
			if (distanceSqr > length * length)
				continue;

			// filter out points that are behind the camera
			if (Vector3.Dot(camTrans.forward, point.transform.position -camTrans.position) < 0) continue;

			// filter out obstructed points
			RaycastHit[] hits = cast(camTrans.position, point.transform.position -camTrans.position, Mathf.Sqrt(distanceSqr) -0.1f);
			if (hits.Length > 0) {
				continue;
			}

			availables.Add(point);
		}
		return availables;
	}
	
	protected static RaycastHit[] cast(Vector3 position, Vector3 direction, float length) {
		RaycastHit[] hits = Physics.RaycastAll(position, direction, length);
		if (hits.Length < 1)
			return new RaycastHit[0];
		RaycastHit finalHit = hits[0];
		foreach(RaycastHit hit in hits) {
			if (hit.distance < finalHit.distance)
				finalHit = hit;
		}
		return new RaycastHit[] {finalHit};
	}

	protected Vector3 lerpVector3(Vector3 start, Vector3 end, float ammount) {
		return start + (end - start).normalized * ammount;
	}
}
