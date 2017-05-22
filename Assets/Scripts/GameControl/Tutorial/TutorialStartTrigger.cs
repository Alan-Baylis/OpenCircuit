using UnityEngine;

public class TutorialStartTrigger : MonoBehaviour {

	public Tutorial tutorial;

	void Start() {
		if (tutorial == null) {
			tutorial = GetComponentInParent<Tutorial>();
		}
	}

	private void OnTriggerEnter(Collider other) {
		Player player = other.GetComponent<Player>();
		if (player != null && player.isLocalPlayer) {
			tutorial.startPlayer(player);
		}
	}
}
