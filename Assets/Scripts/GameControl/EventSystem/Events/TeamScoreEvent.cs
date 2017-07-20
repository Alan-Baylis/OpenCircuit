public class TeamScoreEvent : AbstractEvent {
	private int teamId;
	private float value;

	public TeamScoreEvent(int teamId, float value) {
		this.teamId = teamId;
		this.value = value;
	}

	public float getScore() {
		return value;
	}

	public int getTeam() {
		return teamId;
	}
}
