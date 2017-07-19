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
	    if (endConditionMet()) {
		    onGameOver();
		    GlobalConfig.globalConfig.endGame();
		    gameOver = true;
	    }
	}

    [Server]
    public virtual void initialize() { }

    [Server]
	public abstract bool endConditionMet();

	[Server]
    public abstract void onPlayerDeath(Player player);

	[Server]
    public abstract void onPlayerRevive(Player player);

	[Server]
	public abstract void onGameOver();

	public abstract AbstractPlayerSpawner getPlayerSpawner(ClientController controller);
}
