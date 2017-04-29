using UnityEngine.Networking;

public abstract class GameMode : NetworkBehaviour {

	[SyncVar]
    private bool gameOver = false;

	public bool isGameOver {
		get { return gameOver; }
	}

	public enum GameModes {
		BASES, SPAWNER_HUNT
	}

    [ServerCallback]
    public virtual void Start() {
        NetworkServer.SpawnObjects();
    }

    [ServerCallback]
	protected virtual void Update() {
        if (gameOver)
            return;
	    if (loseConditionMet()) {
		    onLoseGame();
	      GlobalConfig.globalConfig.loseGame();
	        gameOver = true;
	    } else if (winConditionMet()) {
		    onWinGame();
			GlobalConfig.globalConfig.winGame();
	        gameOver = true;
	    }
	}

    [Server]
    public virtual void initialize() { }

    [Server]
	public abstract bool winConditionMet();

    [Server]
    public abstract bool loseConditionMet();

	[Server]
    public abstract void onPlayerDeath(Player player);

	[Server]
    public abstract void onPlayerRevive(Player player);

	[Server]
	public virtual void onWinGame() {
	}

	[Server]
	public virtual void onLoseGame() {
	}
}
