using UnityEngine;

public class Teleport : MonoBehaviour {

	public GameObject target;

	private void OnTriggerEnter(Collider other) {
		Player player = other.transform.root.GetComponent<Player>();
		if (player != null) {
			player.transform.position = target.transform.position;
		}
	}
}
