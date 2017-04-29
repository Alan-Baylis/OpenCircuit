using UnityEngine;

public class TutorialTrigger : MonoBehaviour {

	public Tutorial tutorial;

	private void OnTriggerEnter(Collider other) {
		Player player = other.transform.root.GetComponent<Player>();
		print(player);
		if (player != null && player.isLocalPlayer) {
			tutorial.nextMessage();
			player.inventory.unequip();
		}
	}
}
