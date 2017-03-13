using UnityEngine;
using UnityEngine.Networking;

public abstract class GameMode : NetworkBehaviour {

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

	void Update() {
		if (winConditionMet()) {
			GlobalConfig.globalConfig.winGame();
		}
	}

	public abstract bool winConditionMet();

}
