using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SceneData {

	public string path;
	public GlobalConfigData configuration;
	public List<GameMode.GameModes> supportedGameModes;

	public SceneData (string path, GlobalConfigData configuration) {
		this.path = path;
		this.configuration = configuration;
		supportedGameModes = new List<GameMode.GameModes>();
	}

	public bool isLoadingScene () {
		return supportedGameModes == null || supportedGameModes.Count == 0;
	}

}
