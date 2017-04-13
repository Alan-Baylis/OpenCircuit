using UnityEngine.Networking;

public class Team : NetworkBehaviour {

	public bool autoInitializeTeam;
	public int teamIndex;

    [SyncVar]
    public TeamData team;

	[SyncVar(hook = "isEnabledSet")]
	private bool isEnabled;

    [ServerCallback]
    void Start() {
	    if (autoInitializeTeam) {
		    TeamGameMode teamGameMode = GlobalConfig.globalConfig.gamemode as TeamGameMode;
		    if (teamGameMode != null) {
			    team = teamGameMode.teams[teamIndex];
		    }
	    }

        Label label = GetComponent<Label>();
        label.setTag(new Tag(TagEnum.Team, 0, label.labelHandle));
    }

	[ServerCallback]
	void OnEnable() {
		isEnabled = true;
	}

	[ServerCallback]
	void OnDisable() {
		isEnabled = false;
	}

	[Client]
	private void isEnabledSet(bool enabled) {
		isEnabled = enabled;
		this.enabled = enabled;

	}
}
