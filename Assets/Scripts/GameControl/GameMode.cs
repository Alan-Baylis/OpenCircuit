using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class GameMode : NetworkBehaviour {

	public enum GameModes {
		BASES, SPAWNER_HUNT
	}

	void Update() {
		if (winConditionMet()) {
			GlobalConfig.globalConfig.winGame();
		}
	}

	public abstract bool winConditionMet();

}
