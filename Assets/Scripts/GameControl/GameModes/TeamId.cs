﻿using UnityEngine.Networking;

public class TeamId : NetworkBehaviour {

	[SyncVar]
	public int id;

	[SyncVar(hook = "isEnabledSet")]
	private bool isEnabled;

	public Team team {
		get { return GlobalConfig.globalConfig.teamGameMode.teams[id]; }
	}

    [ServerCallback]
    void Start() {
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
