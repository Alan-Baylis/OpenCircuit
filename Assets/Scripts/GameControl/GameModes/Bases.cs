using UnityEngine;
using UnityEngine.Networking;

public class Bases : TeamGameMode {

    void Start() {
        teams.Add(new TeamData(0, new Color(0, 0, .4777f, 1), TagEnum.Team1));
        teams.Add(new TeamData(1, new Color(.4777f, 0, 0, 1), TagEnum.Team2));
        localTeam = teams[0];
    }

    [Server]
	public override bool winConditionMet() {
		return false;
	}

    [Server]
    public override bool loseConditionMet() {
        return GlobalConfig.globalConfig.frozenPlayers > 0 && GlobalConfig.globalConfig.frozenPlayers >= ClientController.numPlayers;
    }
}
