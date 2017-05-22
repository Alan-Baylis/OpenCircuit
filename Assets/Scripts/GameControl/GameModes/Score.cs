using UnityEngine;

public class Score : MonoBehaviour {

	public float value;
	public ClientController owner;
	public bool teamOwned;

	void Start() {
		Label label = GetComponent<Label>();
		if (label != null) {
			label.addOperation(new CountScoreOperation(), new[] {typeof(DestructTrigger)});
		} else {
			Debug.LogWarning("Score component attached to '" + name + "' requires a label!");
		}
	}

	public void recordScore(ClientController destroyer) {
			if (GetComponent<TeamId>().id == 0) {
				EventManager.broadcastEvent(new ScoreEvent(destroyer, -value), EventManager.IN_GAME_CHANNEL);
			} else {
				EventManager.broadcastEvent(new ScoreEvent(destroyer, value), EventManager.IN_GAME_CHANNEL);
			}
	}

	void OnDestroy() {
		if (teamOwned) {
			EventManager.broadcastEvent(new TeamScoreEvent(GetComponent<TeamId>().id, -value), EventManager.IN_GAME_CHANNEL);
		} else if (owner != null) {
			EventManager.broadcastEvent(new ScoreEvent(owner, -value), EventManager.IN_GAME_CHANNEL);
		}
	}
}
