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
		Bases bases = GlobalConfig.globalConfig.gamemode as Bases;
		if (bases != null) {
			if (GetComponent<TeamId>().id == 0) {
				bases.addScore(destroyer, -value);
			} else {
				bases.addScore(destroyer, value);
			}
		}
	}

	void OnDestroy() {
		Bases bases = GlobalConfig.globalConfig.gamemode as Bases;
		if (bases != null) {
			if (teamOwned) {
				bases.addTeamScore(GetComponent<TeamId>().id, -value);
			} else if (owner != null) {
				bases.addScore(owner, -value);
			}
		}
	}
}
