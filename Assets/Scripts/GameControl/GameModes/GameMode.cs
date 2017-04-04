using UnityEngine;
using UnityEngine.Networking;

public abstract class GameMode : NetworkBehaviour {

    private bool gameOver = false;

	public enum GameModes {
		BASES, SPAWNER_HUNT
	}

    [ServerCallback]
    public void Start() {
        NetworkServer.SpawnObjects();
    }

    [ServerCallback]
	void Update() {
        if (gameOver)
            return;
	    if (loseConditionMet()) {
	      GlobalConfig.globalConfig.loseGame();
	        gameOver = true;
	    } else if (winConditionMet()) {
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

}
