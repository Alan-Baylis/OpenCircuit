using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine.Networking.NetworkSystem;

[AddComponentMenu("Scripts/Menu/Menu")]
[RequireComponent(typeof(NetworkDiscovery))]
public class Menu : MonoBehaviour {

	private Rect topRect = new Rect(0.05f, 0.15f, 0.5f, 0.07f);
	private Rect joinRect = new Rect(0.05f, 0.25f, 0.5f, 0.07f);
	private Rect optionsRect = new Rect(0.05f, 0.35f, 0.5f, 0.07f);
	private Rect adminRect = new Rect(0.05f, 0.45f, 0.5f, 0.07f);
	private Rect spectateRect = new Rect(0.05f, 0.55f, 0.5f, 0.07f);
	private Rect exitRect = new Rect(0.05f, 0.65f, 0.5f, 0.07f);
	private Rect backRect = new Rect(0.05f, 0.8f, 0.5f, 0.07f);
	private Rect titleRect = new Rect(0.05f, 0.05f, 0.75f, 0.1f);
	private state currentMenu = state.MainMenu;
	private Stack<state> menuHistory = new Stack<state>();
	private string host = "localhost";
	private string username;
	private Vector2 scrollPosition = Vector2.zero;
	private MessageDialog currentDialogBox;

	private NetworkDiscovery nd;
	private NetworkDiscovery networkDiscovery { get {
		if (nd == null)
			nd = GetComponent<NetworkDiscovery>();
		return nd;
	} }

    private bool isHost;

	public MessageDialog dialogBoxPrefab;
    public GlobalConfig globalConfigPrefab;

	[System.NonSerialized]
	public GlobalConfigData serverConfig = GlobalConfigData.getDefault();
	public float defaultScreenHeight = 1080;
	public bool activeAtStart = true;
	public GUISkin skin;
	public Texture2D background;
	//public Texture2D controls;
	public Vector3 endCamPosition;
	public Vector3 endCamRotation;

	private static Menu myMenu = null;
	public static Menu menu { get {
			return myMenu;
	}}
	
	private enum state {
		MainMenu, InGameMenu, Options, Host, Join, Win, Lose, ClientLobby
	};

	public bool paused() {
		return activeAtStart;
	}

	public void toggleInGameMenu() {
		if (paused()) {
			unpause();
		} else {
			pause();
			currentMenu = state.InGameMenu;
		}
	}

	public void pause() {
		if (paused()) return;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		activeAtStart = true;
	}

	public void unpause() {
		if (!paused()) return;
		Cursor.lockState = CursorLockMode.Locked;
		activeAtStart = false;
		menuHistory.Clear();
	}

	// Use this for initialization
	public void Start() {
	    DontDestroyOnLoad(gameObject);
	    myMenu = this;
		username = System.Environment.MachineName.ToLower();
		if (activeAtStart) {
			pause();
			currentMenu = state.MainMenu;
		}
    }

	public void OnGUI() {
		if (!activeAtStart) return;
		GUI.depth = -1;
		GUI.skin = skin;
		float menuWidth = 0.6f;
		float width = background.width / background.height;
		GUI.DrawTexture(GUIUtil.convertRect(new Rect(menuWidth -width, 0, width, 1), false), background);
	    switch (currentMenu) {
	        case state.MainMenu:
	            doMainMenu();
	            break;
	        case state.InGameMenu:
	            doInGameMenu();
	            break;
	        case state.Options:
	            doOptions();
	            break;
	        case state.Host:
	            doHost();
	            break;
	        case state.Join:
	            doJoin();
	            break;
	        case state.ClientLobby:
	            doLobby();
	            break;
	        case state.Win:
	            doWin();
	            break;
	        case state.Lose:
	            doLose();
	            break;
	    }
	}

	public void win() {
		pause();
		currentMenu = state.Win;
	}

	public void lose() {
		pause();
        currentMenu = state.Lose;
    }

	private void doLose() {
		GUIUtil.adjustFontSize(skin.button, exitRect.height * 0.8f);
		if (GUIUtil.button("To Lobby", exitRect, skin.button)) {
			Destroy(currentDialogBox);
			returnToLobby();
		} else if (currentDialogBox == null) {
				currentDialogBox = Instantiate(dialogBoxPrefab);
				currentDialogBox.message = "Critical Failure.";
		}
	}

	private void doWin() {
		GUIUtil.adjustFontSize(skin.button, exitRect.height *0.8f);
		if (GUIUtil.button("To Lobby", exitRect, skin.button)) {
			Destroy(currentDialogBox);
		    returnToLobby();
		}else if (currentDialogBox == null) {
			currentDialogBox = Instantiate(dialogBoxPrefab);
			currentDialogBox.message = "Domination Achieved.";
		}
	}

	private void doInGameMenu() {
		GUIUtil.adjustFontSize(skin.button, topRect.height *0.8f);
		if (GUIUtil.button("Resume", topRect, skin.button)) {
			toggleInGameMenu();
		}
		if (GUIUtil.button("Drop out", joinRect, skin.button)) {
			dropOut();
		}
//		GUIUtil.adjustFontSize(skin.button, exitRect.height * 0.8f);
//		if (GUIUtil.button("Quit", exitRect, skin.button)) {
//            quit();
//		}
		GUIUtil.adjustFontSize(skin.button, optionsRect.height * 0.8f);
		if (GUIUtil.button("Options", optionsRect, skin.button)) {
			menuHistory.Push(currentMenu);
			currentMenu = state.Options;
		}
	}

	private void doMainMenu() {
		// draw title
		GUIUtil.adjustFontSize(skin.label, titleRect.height);
		GUI.Label(GUIUtil.convertRect(titleRect, false), "Guns 'n' Robots", skin.label);

		GUIUtil.adjustFontSize(skin.button, topRect.height * 0.8f);
		if (GUIUtil.button("Host", topRect, skin.button)) {
			menuHistory.Push(currentMenu);
			currentMenu = state.Host;
		    SceneData ? activeSceneData = SceneCatalog.sceneCatalog.getSceneData(SceneManager.GetActiveScene().path);
		    if (activeSceneData != null) {
		        serverConfig = activeSceneData.Value.configuration;
		    }
		    startListen();
		}
		GUIUtil.adjustFontSize(skin.button, joinRect.height * 0.8f);
		if(GUIUtil.button("Join", joinRect, skin.button)) {
			menuHistory.Push(currentMenu);
			currentMenu = state.Join;
			networkDiscovery.Initialize();
			networkDiscovery.StartAsClient();
		}
		GUIUtil.adjustFontSize(skin.button, optionsRect.height * 0.8f);
		if (GUIUtil.button("Options", optionsRect, skin.button)) {
			menuHistory.Push(currentMenu);
			currentMenu = state.Options;
		}
		GUIUtil.adjustFontSize(skin.button, exitRect.height * 0.8f);
		if (GUIUtil.button("Quit", exitRect, skin.button)) {
            quit();
		}
	}

	private void doOptions() {

		// graphics settings

		// shadow distance
		GUIUtil.adjustFontSize(skin.label, 0.05f);
		GUI.Label(GUIUtil.convertRect(new Rect(0.05f, 0.1f, 0.4f, 0.05f), false), "Shadow Distance:  " +QualitySettings.shadowDistance.ToString("##,0.") +" m");
		QualitySettings.shadowDistance = GUI.HorizontalSlider(GUIUtil.convertRect(new Rect(0.05f, 0.16f, 0.25f, 0.04f), false), QualitySettings.shadowDistance, 0, 200);

		// vsync setting
		string[] vSyncOptions = { "None", "Full", "Half" };
		GUIUtil.adjustFontSize(skin.label, 0.05f);
		GUI.Label(GUIUtil.convertRect(new Rect(0.05f, 0.18f, 0.2f, 0.05f), false), "VSync: " +vSyncOptions[QualitySettings.vSyncCount]);
		//QualitySettings.vSyncCount = GUI.SelectionGrid(convertRect(new Rect(0.1f, 0.2f, 0.3f, 0.04f)), QualitySettings.vSyncCount, vSyncOptions, 3);
		QualitySettings.vSyncCount = (int)(GUI.HorizontalSlider(GUIUtil.convertRect(new Rect(0.05f, 0.24f, 0.2f, 0.04f), false), QualitySettings.vSyncCount, 0, 2) +0.5f);


		// input settings

		// look sensitivity
		//Player myPlayer = player;
		//adjustFontSize(skin.label, 0.07f);
		//GUI.Label(convertRect(new Rect(0.05f, 0.28f, 0.4f, 0.07f), false), "Look Sensitivity:  " +(myPlayer.controls.mouseSensitivity *4).ToString("##,0.0#"));
		//myPlayer.controls.mouseSensitivity = GUI.HorizontalSlider(convertRect(new Rect(0.05f, 0.34f, 0.25f, 0.04f), false), myPlayer.controls.mouseSensitivity, 0.0625f, 2);
		//myPlayer.controls.mouseSensitivity = ((int)(myPlayer.controls.mouseSensitivity * 16 + 0.5f)) / 16f;

		// Y axis look inversion
		//GUI.Label(convertRect(new Rect(0.05f, 0.36f, 0.4f, 0.07f), false), "Look Inversion: ");
		//myPlayer.controls.invertLook = GUI.Toggle(convertRect(new Rect(0.25f, 0.38f, 0.25f, 0.07f), false), myPlayer.controls.invertLook, "");
		

		// back button
		GUIUtil.adjustFontSize(skin.button, backRect.height);
		if (GUIUtil.button("Back", backRect, skin.button)) {
			currentMenu = menuHistory.Pop();
		}
	}

	private void doHost() {	
		GUIUtil.adjustFontSize(skin.label, 0.07f);
		GUI.Label(GUIUtil.convertRect(new Rect(0.05f, 0.05f, 0.5f, 0.07f), false), "Configure Server");

		// start button
		GUIUtil.adjustFontSize(skin.button, topRect.height * 0.8f);
		if (GUIUtil.button("Start Game", topRect)) {
			begin();
        }

		// configuration
		GUIUtil.adjustFontSize(skin.label, 0.03f);
		GUIUtil.adjustFontSize(skin.textField, 0.03f);
		GUIUtil.adjustFontSize(skin.button, 0.03f);
//		GUI.Label(GUIUtil.convertRect(new Rect(0.05f, 0.3f, 0.2f, 0.03f), false), "Server Name: ");
//		serverName = GUI.TextField(GUIUtil.convertRect(new Rect(0.25f, 0.3f, 0.3f, 0.03f), false), serverName);
		GUI.Label(GUIUtil.convertRect(new Rect(0.05f, 0.3f, 0.3f, 0.03f), false), "Robot Spawn Rate: ");
		serverConfig.robotSpawnRatePerSecond = GUIUtil.numberField(new Rect(0.35f, 0.3f, 0.2f, 0.03f), serverConfig.robotSpawnRatePerSecond);
		GUI.Label(GUIUtil.convertRect(new Rect(0.05f, 0.35f, 0.3f, 0.03f), false), "Spawn Rate Increase: ");
		serverConfig.spawnRateIncreasePerPlayer = GUIUtil.numberField(new Rect(0.35f, 0.35f, 0.2f, 0.03f), serverConfig.spawnRateIncreasePerPlayer);
		GUI.Label(GUIUtil.convertRect(new Rect(0.05f, 0.4f, 0.3f, 0.03f), false), "Robots per Team: ");
		serverConfig.robotsPerPlayer = GUIUtil.numberField(new Rect(0.35f, 0.4f, 0.2f, 0.03f), serverConfig.robotsPerPlayer);
		GameMode.GameModes [] modes = (GameMode.GameModes[])System.Enum.GetValues(typeof(GameMode.GameModes));
		List<string> modeStrings = new List<string>();
		foreach (GameMode.GameModes mode in modes) {
			modeStrings.Add(mode.ToString());
		}

//		int returnValue = GUIUtil.dropDownSelector(new Rect(0.05f, 0.60f, 0.5f, 0.05f), modeStrings, (int)serverConfig.gameMode);
//	    GameMode.GameModes selectedMode =
//	        (GameMode.GameModes) System.Enum.Parse(typeof(GameMode.GameModes), modeStrings[returnValue]);
//	    if (selectedMode != serverConfig.gameMode) {
//	        loadDefaultSceneConfigurationFor(SceneCatalog.sceneCatalog.getScenesForGameMode(selectedMode)[0].path);
//	        serverConfig.gameMode = selectedMode;
//	    }
		serverConfig.gameMode = GameMode.GameModes.BASES;
	    // back button
		//GUIUtil.adjustFontSize(skin.button, backRect.height * 0.8f);
		//if (GUIUtil.button("Cancel", backRect, skin.button)) {
		//	currentMenu = menuHistory.Pop();
		//	stopListen();
		//}
	}

	private void doJoin() {
		GUIUtil.adjustFontSize(skin.label, 0.07f);
		GUI.Label(GUIUtil.convertRect(new Rect(0.05f, 0.05f, 0.5f, 0.07f), false), "LAN Servers");

		// server list
		List<NetworkBroadcastResult> servers =
			new List<NetworkBroadcastResult>(networkDiscovery.broadcastsReceived.Values);
		Rect pos = GUIUtil.convertRect(new Rect(0.05f, 0.12f, 0.5f, 0.5f), false);
		GUI.BeginGroup(pos, skin.box);
        scrollPosition = GUI.BeginScrollView(new Rect(0, 0, pos.width, pos.height), scrollPosition,
			GUIUtil.convertRect(new Rect(0, 0, 0.5f, Mathf.Max(0.05f *servers.Count, 0.2f)), false));
		GUIUtil.adjustFontSize(skin.button, 0.04f);
		int position = 0;
		foreach(NetworkBroadcastResult server in servers) {
			string serverName = System.Text.Encoding.Unicode.GetString(server.broadcastData);  
			if (GUIUtil.button(serverName + "   -   " + server.serverAddress, new Rect(0, 0.05f * position, 0.5f, 0.05f))) {
				host = server.serverAddress;
				join();
				return;
			}
		}
		GUI.EndScrollView(true);
		GUI.EndGroup();

		// join custom address
		GUIUtil.adjustFontSize(skin.textArea, 0.05f);
		host = GUI.TextField(GUIUtil.convertRect(new Rect(0.2f, 0.65f, 0.35f, 0.05f), false), host);
		GUIUtil.adjustFontSize(skin.button, 0.05f);
		if (GUIUtil.button("Join", new Rect(0.05f, 0.64f, 0.15f, 0.07f))) {
			join();
		}

		// back button
		GUIUtil.adjustFontSize(skin.button, backRect.height);
		if (GUIUtil.button("Back", backRect)) {
			networkDiscovery.StopBroadcast();
			currentMenu = menuHistory.Pop();
		}
	}

    private void doLobby() {
        GUIUtil.adjustFontSize(skin.button, exitRect.height * 0.8f);
        if (GlobalConfig.globalConfig != null && GlobalConfig.globalConfig.gameStarted && !GlobalConfig.globalConfig.gamemode.isGameOver) {
	        GUIUtil.adjustFontSize(skin.label, 0.03f);
			GUI.Label(GUIUtil.convertRect(new Rect(0.05f, 0.3f, 0.2f, 0.03f), false), "Player Name: ");
	        username = GUI.TextField(GUIUtil.convertRect(new Rect(0.25f, 0.3f, 0.3f, 0.03f), false), username).ToLower();
	        //TODO build this specially ;)
//	        if (GUIUtil.button("Admin", adminRect, skin.button)) {
//		        dropIn(ClientType.ADMIN);
//	        }
			if(GUIUtil.button("Spectate", spectateRect, skin.button)) {
		        dropIn(ClientType.SPECTATOR);
	        }
			if (GUIUtil.button("Drop In", exitRect, skin.button)) {
				dropIn(ClientType.PLAYER);
            }
        } else {
			GUIUtil.adjustFontSize(skin.label, 0.07f);
			GUI.Label(GUIUtil.convertRect(new Rect(0.05f, 0.05f, 0.5f, 0.07f), false), "Waiting on host...");
        }
    }

	private void join() {
		NetworkManager manager = NetworkManager.singleton;
	    manager.networkAddress = host;
	    NetworkController.networkController.connect();
	    currentMenu = state.ClientLobby;
		networkDiscovery.StopBroadcast();
	}

	private void begin() {
	    isHost = true;
		string activeScenePath = SceneManager.GetActiveScene().path;
		SceneData? sceneData = SceneCatalog.sceneCatalog.getSceneData(activeScenePath);
	    if (sceneData == null || !sceneData.Value.supportedGameModes.Contains(serverConfig.gameMode)) {
	        List<SceneData> scenes = SceneCatalog.sceneCatalog.getScenesForGameMode(serverConfig.gameMode);
	        if (scenes.Count == 0) {
	            return;
	        }
	        NetworkController.networkController.serverChangeScene(scenes[0].path);
	        StartCoroutine("startGameWhenReady");
	    } else if (sceneData != null && sceneData.Value.supportedGameModes.Contains(serverConfig.gameMode)) {
            startGame();
	    }
	}

	private void dropIn(ClientType type) {
		string roleCode = ((int)type).ToString();
		StringMessage message = new StringMessage(roleCode + username);

		ClientScene.AddPlayer(null, 0, message);
		activeAtStart = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void dropOut() {
		ClientScene.RemovePlayer(0);
		activeAtStart = true;
		currentMenu = state.ClientLobby;
		Cursor.lockState = CursorLockMode.None;
	}

    private void returnToLobby() {
        menuHistory.Clear();
        activeAtStart = true;
        if (isHost) {
            currentMenu = state.Host;
            NetworkController.networkController.serverChangeScene(SceneManager.GetActiveScene().path);
        } else {
            currentMenu = state.ClientLobby;
        }
    }

	private void quit() {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void startListen() {
        NetworkController.networkController.listen();
        if (!networkDiscovery.running) {
            networkDiscovery.Initialize();
	        networkDiscovery.broadcastData = System.Environment.MachineName;
            networkDiscovery.StartAsServer();
	        networkDiscovery.Initialize();
        }
    }

	private void stopListen() {
		networkDiscovery.StopBroadcast();
		NetworkController.networkController.stopListening();
	}

    IEnumerator startGameWhenReady() {
        while (!NetworkController.networkController.allClientsReady()) {
            yield return null;
        }
        startGame();
    }

    private void startGame() {
        //NetworkManager manager = NetworkManager.singleton;
        //if (!manager.isNetworkActive) {
            //manager.StartHost();
        //}
        //player.gameObject.SetActive(true);
        //GetComponent<Camera>().enabled = false;
        //GetComponent<AudioListener>().enabled = false;
        //NetworkServer.SpawnObjects();
	    GlobalConfig [] globalConfigs = Resources.FindObjectsOfTypeAll<GlobalConfig>();

	    GlobalConfig globalConfig;
	    if (globalConfigs.Length == 1) {
		    globalConfig = Instantiate(globalConfigPrefab);
	    } else {
		    globalConfig = globalConfigs[0].gameObject.scene.name == null ? globalConfigs[1] : globalConfigs[0];
		    globalConfig.gameObject.SetActive(true);
	    }

	    NetworkServer.Spawn(globalConfig.gameObject);
        menuHistory.Clear();
        currentMenu = state.ClientLobby;
    }

    private void loadDefaultSceneConfigurationFor(string path) {
        SceneCatalog sceneCatalog = SceneCatalog.sceneCatalog;
        SceneData? sceneData = sceneCatalog.getSceneData(path);
        if (sceneData != null)
            menu.serverConfig = sceneData.Value.configuration;
    }

	private enum ClientType {
		PLAYER,SPECTATOR,ADMIN
	}
}
