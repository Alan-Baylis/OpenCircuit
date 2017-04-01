using UnityEngine;
using UnityEngine.Networking;

public abstract class GameMode : NetworkBehaviour {

    private bool gameOver = false;

	public enum GameModes {
		BASES, SPAWNER_HUNT
	}

    public static GameMode constructGameMode(GameObject target, GameModes gameType) {
        //TODO: Do this better
        GameMode mode = null;
        switch (gameType) {
            case GameMode.GameModes.BASES:
                mode = target.AddComponent<Bases>();
                break;
            case GameMode.GameModes.SPAWNER_HUNT:
                mode = target.AddComponent<SpawnerHunt>();
                break;
        }
        return mode;
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
	public abstract bool winConditionMet();

    [Server]
    public abstract bool loseConditionMet();

}
