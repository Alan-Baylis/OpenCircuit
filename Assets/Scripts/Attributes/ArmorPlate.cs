using UnityEngine;
using System.Collections;

public class ArmorPlate : Health {

	public float cleanupTime = 60;
	private Armor armor;

	public override void destruct() {
		transform.parent = null;
		if (armor != null) {
			armor.plateDestroyed(this);
		}

		Rigidbody rb = GetComponent<Rigidbody>();
		if (rb == null) {
			Destroy(this);
			return;
		}

		Collider[] cols = GetComponents<Collider>();
		foreach (Collider col in cols) {
			if (col.isTrigger) {
				continue;
			} else {
				if (col as MeshCollider != null)
					((MeshCollider)col).convex = true;
				col.enabled = true;
			}
		}

		rb.isKinematic = false;
		rb.useGravity = true;
		const float maxForce = 50;
		rb.AddForce(randomRange(Vector3.one * -maxForce, Vector3.one * maxForce));
		Destroy(this, cleanupTime);
	}

	public void setArmor(Armor armor) {
		this.armor = armor;
	}

	protected static Vector3 randomRange(Vector3 min, Vector3 max) {
		return new Vector3(
			UnityEngine.Random.Range(min.x, max.x),
			UnityEngine.Random.Range(min.y, max.y),
			UnityEngine.Random.Range(min.z, max.z));
	}
}
