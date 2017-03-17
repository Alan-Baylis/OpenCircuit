using System.Collections;
using UnityEngine;

public class LogoPiece : MonoBehaviour {

	public float detachTime;

	private Rigidbody rb;

	private Transform firstPoint;

	public void Start () {
		rb = GetComponent<Rigidbody>();
	}

	public void OnCollisionEnter(Collision col) {
		Rigidbody other = col.rigidbody;
		LogoLetter letter = col.gameObject.GetComponentInParent<LogoLetter>();
		if (letter != null) {
			Physics.IgnoreCollision(GetComponent<Collider>(), col.collider);

			if (firstPoint == null) {
				int hits = letter.getHits(col.transform);
				if (Random.Range(0f, 1f) <= 0.8f - 0.7f / Mathf.Max(hits, 1))
					return;
				firstPoint = col.transform;
				rb.angularVelocity = rb.angularVelocity * 100000;

				HingeJoint joint = gameObject.AddComponent<HingeJoint>();
				joint.autoConfigureConnectedAnchor = false;

				joint.connectedBody = other;
				joint.connectedAnchor = col.transform.InverseTransformPoint(col.contacts[0].point);
				joint.anchor = transform.InverseTransformPoint(col.contacts[0].point);
				joint.axis = transform.InverseTransformVector(new Vector3(0, 0, 1));
				StartCoroutine("detach");
			} else {
				if (!letter.isNeighbor(col.transform, firstPoint))
					return;
				foreach (HingeJoint hinge in GetComponents<HingeJoint>()) {
					Destroy(hinge);
				}
				Destroy(rb);
			}
			letter.addHit(col.transform);
			letter.addHit(col.transform);
		} else {
			Destroy(gameObject);
		}
	}

	private IEnumerator detach() {
		yield return new WaitForSeconds(detachTime);

		firstPoint = null;
		foreach (HingeJoint joint in GetComponents<HingeJoint>()) {
			Destroy(joint);
		}
	}
}
