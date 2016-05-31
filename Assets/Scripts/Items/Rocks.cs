using UnityEngine;
using System.Collections;

[AddComponentMenu("Scripts/Items/Rocks")]
public class Rocks : Item {

	public Vector3 throwVelocity = new Vector3(0, 2f, 15);
	public Vector3 releaseLocation = new Vector3(0.4f, 0.3f, 0.3f);
	public Mesh rockMesh;
	public Material rockMaterial;
	public AudioClip collisionSound;
	

	public override void beginInvoke(Inventory invoker) {
		Transform cam = invoker.getPlayer().cam.transform;
        GameObject rock = new GameObject("Rock");
		rock.transform.localPosition = cam.TransformPoint(releaseLocation);
		rock.transform.localScale *= 0.2f;
		rock.AddComponent<MeshFilter>().sharedMesh = rockMesh;
		rock.AddComponent<MeshRenderer>().sharedMaterial = rockMaterial;
		Rigidbody rb = rock.AddComponent<Rigidbody>();
		rb.velocity = cam.TransformVector(throwVelocity);
		rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
		rock.AddComponent<Rock>().clip = collisionSound;
	}

	public override void onEquip(Inventory equipper) {
		base.onEquip(equipper);
	}

	public override void onUnequip(Inventory equipper) {
		base.onUnequip(equipper);
	}
}
