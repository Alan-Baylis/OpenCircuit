using System.Collections.Generic;

public abstract class TeamGameMode : GameMode {

    public int localTeamId;

    public Team.Config[] teamConfig = new Team.Config[0];

	[System.NonSerialized]
	public Dictionary<int, Team> teams;

	public override void initialize() {
		teams = new Dictionary<int, Team>();
		for (int id=0; id<teamConfig.Length; ++id) {
			teams[id] = new Team(id, teamConfig[id]);
		}
	}

	public abstract int getMaxRobots(int teamIndex);

	public abstract int getJoinedPlayerCount(int teamIndex);

}
