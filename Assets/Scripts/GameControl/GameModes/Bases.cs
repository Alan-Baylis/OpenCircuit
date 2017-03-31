public class Bases : GameMode {
	public override bool winConditionMet() {
		return false;
	}

    public override bool loseConditionMet() {
        return GlobalConfig.globalConfig.frozenPlayers >= ClientController.numPlayers;
    }
}
