using System.Collections;
using UnityEngine;

public class OnDamageTrigger : MonoBehaviour {

	public Tutorial tutorial;
	public AbstractPlayerSpawner spawner;
	public float timer = .25f;

	private Player player;

	void Start() {
	}

	public void doTheThing(Player player) {
		if (enabled && this.player == null) {
			this.player = player;
			StartCoroutine("teleportPlayer");
		}
	}

	IEnumerator teleportPlayer() {
		yield return new WaitForSeconds(timer);
		tutorial.nextMessage();
		player.transform.position = spawner.nextSpawnPos();
	}
}
