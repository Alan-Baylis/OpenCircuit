public class ScoreEvent : AbstractEvent {

	private ClientController owner;
	private float value;

	public ScoreEvent(ClientController owner, float value) {
		this.owner = owner;
		this.value = value;
	}

	public float getScore() {
		return value;
	}

	public ClientController getOwner() {
		return owner;
	}

}
