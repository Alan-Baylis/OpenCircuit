using UnityEngine;
using UnityEngine.Networking;

public class DismantleEffect {

	public static void dismantle(Transform trans, float lingerTime, bool isServer, Vector3 velocity) {
		trans.parent = null;
		trans.gameObject.hideFlags |= HideFlags.HideInHierarchy;
		while (trans.childCount > 0)
			dismantle(trans.GetChild(0), lingerTime, isServer, velocity);
		Collider [] cols = trans.GetComponents<Collider>();

		bool hasCollider = false;
		foreach (Collider col in cols) {

			if (col.isTrigger) {
				continue;
			} else {
				hasCollider = true;
				if (col as MeshCollider != null)
					((MeshCollider)col).convex = true;
				col.enabled = true;
			}
		}

		if (isServer || trans.GetComponent<NetworkIdentity>() == null) {
			if (hasCollider) {
				useAsTemporaryDebris(trans, lingerTime, velocity);
			} else {
				MonoBehaviour.Destroy(trans.gameObject);
			}
		} else if (hasCollider) {
			convertToDebris(trans, velocity);
		}
	}

	protected static void useAsTemporaryDebris(Transform trans, float lingerTime, Vector3 velocity) {
		convertToDebris(trans, velocity);
		MonoBehaviour.Destroy(trans.gameObject, lingerTime);
	}

	protected static void convertToDebris(Transform trans, Vector3 velocity) {
		const float maxForce = 200;
		Rigidbody rb = trans.GetComponent<Rigidbody>();
		if (rb == null)
			rb = trans.gameObject.AddComponent<Rigidbody>();
		rb.isKinematic = false;
		rb.useGravity = true;
		rb.velocity = velocity;
		rb.AddForce(randomRange(Vector3.one * -maxForce, Vector3.one * maxForce));
	}

	protected static Vector3 randomRange(Vector3 min, Vector3 max) {
		return new Vector3(
			Random.Range(min.x, max.x),
			Random.Range(min.y, max.y),
			Random.Range(min.z, max.z));
	}
}
