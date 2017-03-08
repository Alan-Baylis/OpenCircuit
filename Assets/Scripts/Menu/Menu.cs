using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections.Generic;

[AddComponentMenu("Scripts/Menu/Menu")]
[RequireComponent(typeof(NetworkDiscovery))]
public class Menu : MonoBehaviour, SceneLoadListener {

	private Rect hostRect = new Rect(0.05f, 0.15f, 0.5f, 0.07f);
	private Rect joinRect = new Rect(0.05f, 0.25f, 0.5f, 0.07f);
	private Rect exitRect = new Rect(0.05f, 0.65f, 0.5f, 0.07f);
	private Rect optionsRect = new Rect(0.05f, 0.35f, 0.5f, 0.07f);
	private Rect backRect = new Rect(0.05f, 0.8f, 0.5f, 0.07f);
	private Rect titleRect = new Rect(0.05f, 0.05f, 0.75f, 0.1f);
	private Rect resumeRect = new Rect(0.05f, 0.15f, 0.5f, 0.07f);
	private state currentMenu = state.MainMenu;
	private Stack<state> menuHistory = new Stack<state>();
	private float endTextFontSize = .2f;
	private string host = "localhost";
	private string serverName = "Lazy Setup";
	private Vector2 scrollPosition = Vector2.zero;
	private NetworkDiscovery nd;
	private NetworkDiscovery networkDiscovery { get {
		if (nd == null)
			nd = GetComponent<NetworkDiscovery>();
		return nd;
	} }


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
		MainMenu, InGameMenu, Options, Host, Join, Win, Lose
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
		GUI.DrawTexture(convertRect(new Rect(menuWidth -width, 0, width, 1), false), background);
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
		adjustFontSize(skin.button, exitRect.height * 0.8f);
		if (GUI.Button(convertRect(exitRect, false), "Quit", skin.button)) {
            quit();
		}
		int width = 400;
		int height = 50;
		Rect position = new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height);
		adjustFontSize(skin.button, endTextFontSize);
		GUI.Label(position, "You Lost!", skin.button);
	}

	private void doWin() {
		adjustFontSize(skin.button, exitRect.height *0.8f);
		if (GUI.Button(convertRect(exitRect, false), "Quit", skin.button)) {
            quit();
		}
		int width = 400;
		int height = 50;
		Rect position = new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height);
		adjustFontSize(skin.button, endTextFontSize);
		GUI.Label(position, "You Won!", skin.button);
	}

	private void doInGameMenu() {
		adjustFontSize(skin.button, resumeRect.height *0.8f);
		if (GUI.Button(convertRect(resumeRect, false), "Resume", skin.button)) {
			toggleInGameMenu();
		}
		adjustFontSize(skin.button, exitRect.height * 0.8f);
		if (GUI.Button(convertRect(exitRect, false), "Quit", skin.button)) {
            quit();
		}
		adjustFontSize(skin.button, optionsRect.height * 0.8f);
		if (GUI.Button(convertRect(optionsRect, false), "Options", skin.button)) {
			menuHistory.Push(currentMenu);
			currentMenu = state.Options;
		}
	}

	private void doMainMenu() {
		// draw title
		adjustFontSize(skin.label, titleRect.height);
		GUI.Label(convertRect(titleRect, false), "Guns 'n' Robots", skin.label);

		adjustFontSize(skin.button, hostRect.height * 0.8f);
		if (GUI.Button(convertRect(hostRect, false), "Host", skin.button)) {
			menuHistory.Push(currentMenu);
			currentMenu = state.Host;
		}
		adjustFontSize(skin.button, joinRect.height * 0.8f);
		if(GUI.Button(convertRect(joinRect, false), "Join", skin.button)) {
			menuHistory.Push(currentMenu);
			currentMenu = state.Join;
			networkDiscovery.Initialize();
			networkDiscovery.StartAsClient();
		}
		adjustFontSize(skin.button, optionsRect.height * 0.8f);
		if (GUI.Button(convertRect(optionsRect, false), "Options", skin.button)) {
			menuHistory.Push(currentMenu);
			currentMenu = state.Options;
		}
		adjustFontSize(skin.button, exitRect.height * 0.8f);
		if (GUI.Button(convertRect(exitRect, false), "Quit", skin.button)) {
            quit();
		}
	}

	private void doOptions() {

		// graphics settings

		// shadow distance
		adjustFontSize(skin.label, 0.05f);
		GUI.Label(convertRect(new Rect(0.05f, 0.1f, 0.4f, 0.05f), false), "Shadow Distance:  " +QualitySettings.shadowDistance.ToString("##,0.") +" m");
		QualitySettings.shadowDistance = GUI.HorizontalSlider(convertRect(new Rect(0.05f, 0.16f, 0.25f, 0.04f), false), QualitySettings.shadowDistance, 0, 200);

		// vsync setting
		string[] vSyncOptions = { "None", "Full", "Half" };
		adjustFontSize(skin.label, 0.05f);
		GUI.Label(convertRect(new Rect(0.05f, 0.18f, 0.2f, 0.05f), false), "VSync: " +vSyncOptions[QualitySettings.vSyncCount]);
		//QualitySettings.vSyncCount = GUI.SelectionGrid(convertRect(new Rect(0.1f, 0.2f, 0.3f, 0.04f)), QualitySettings.vSyncCount, vSyncOptions, 3);
		QualitySettings.vSyncCount = (int)(GUI.HorizontalSlider(convertRect(new Rect(0.05f, 0.24f, 0.2f, 0.04f), false), QualitySettings.vSyncCount, 0, 2) +0.5f);


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
		adjustFontSize(skin.button, backRect.height);
		if (GUI.Button(convertRect(backRect, false), "Back", skin.button)) {
			currentMenu = menuHistory.Pop();
		}
	}

	private void doHost() {	
		adjustFontSize(skin.label, 0.07f);
		GUI.Label(convertRect(new Rect(0.05f, 0.05f, 0.5f, 0.07f), false), "Configure Server");

		// start button
		adjustFontSize(skin.button, hostRect.height * 0.8f);
		if (GUI.Button(convertRect(hostRect, false), "Start Hosting")) {
			begin();
        }

		// configuration
		adjustFontSize(skin.label, 0.03f);
		adjustFontSize(skin.textField, 0.03f);
		adjustFontSize(skin.button, 0.03f);
		GUI.Label(convertRect(new Rect(0.05f, 0.3f, 0.2f, 0.03f), false), "Server Name: ");
		serverName = GUI.TextField(convertRect(new Rect(0.25f, 0.3f, 0.3f, 0.03f), false), serverName);
		GUI.Label(convertRect(new Rect(0.05f, 0.45f, 0.3f, 0.03f), false), "Robot Spawn Rate: ");
		serverConfig.robotSpawnRatePerSecond = numberField(new Rect(0.35f, 0.45f, 0.2f, 0.03f), serverConfig.robotSpawnRatePerSecond);
		GUI.Label(convertRect(new Rect(0.05f, 0.5f, 0.3f, 0.03f), false), "Spawn Rate Increase: ");
		serverConfig.spawnRateIncreasePerPlayer = numberField(new Rect(0.35f, 0.5f, 0.2f, 0.03f), serverConfig.spawnRateIncreasePerPlayer);
		GUI.Label(convertRect(new Rect(0.05f, 0.55f, 0.3f, 0.03f), false), "Robots per Player: ");
		serverConfig.robotsPerPlayer = numberField(new Rect(0.35f, 0.55f, 0.2f, 0.03f), serverConfig.robotsPerPlayer);
		GameMode.GameModes [] modes = (GameMode.GameModes[])System.Enum.GetValues(typeof(GameMode.GameModes));
		List<string> modeStrings = new List<string>();
		foreach (GameMode.GameModes mode in modes) {
			modeStrings.Add(mode.ToString());
		}

		int returnValue = dropDownSelector(new Rect(0.05f, 0.60f, 0.5f, 0.05f), modeStrings, (int)serverConfig.gameMode);
        serverConfig.gameMode = (GameMode.GameModes)System.Enum.Parse(typeof(GameMode.GameModes), modeStrings[returnValue]);

		// back button
		adjustFontSize(skin.button, backRect.height * 0.8f);
		if (GUI.Button(convertRect(backRect, false), "Back", skin.button)) {
			currentMenu = menuHistory.Pop();
		}
	}

	private void doJoin() {
		adjustFontSize(skin.label, 0.07f);
		GUI.Label(convertRect(new Rect(0.05f, 0.05f, 0.5f, 0.07f), false), "LAN Servers");

		// server list
		List<NetworkBroadcastResult> servers =
			new List<NetworkBroadcastResult>(networkDiscovery.broadcastsReceived.Values);
		Rect pos = convertRect(new Rect(0.05f, 0.12f, 0.5f, 0.5f), false);
		GUI.BeginGroup(pos, skin.box);
        scrollPosition = GUI.BeginScrollView(new Rect(0, 0, pos.width, pos.height), scrollPosition,
			convertRect(new Rect(0, 0, 0.5f, Mathf.Max(0.05f *servers.Count, 0.2f)), false));
		adjustFontSize(skin.button, 0.04f);
		int position = 0;
		foreach(NetworkBroadcastResult server in servers) {
			string serverName = System.Text.Encoding.Unicode.GetString(server.broadcastData);  
			if (GUI.Button(convertRect(new Rect(0, 0.05f * position, 0.5f, 0.05f), false), serverName + "   -   " + server.serverAddress)) {
				host = server.serverAddress;
				join();
				return;
			}
		}
		GUI.EndScrollView(true);
		GUI.EndGroup();

		// join custom address
		adjustFontSize(skin.textArea, 0.05f);
		host = GUI.TextField(convertRect(new Rect(0.2f, 0.65f, 0.35f, 0.05f), false), host);
		adjustFontSize(skin.button, 0.05f);
		if (GUI.Button(convertRect(new Rect(0.05f, 0.64f, 0.15f, 0.07f), false), "Join")) {
			join();
		}

		// back button
		adjustFontSize(skin.button, backRect.height);
		if (GUI.Button(convertRect(backRect, false), "Back")) {
			networkDiscovery.StopBroadcast();
			currentMenu = menuHistory.Pop();
		}
	}

	private int dropDownSelector(Rect relativePosition, List<string> options, int startValue) {
		return GUI.SelectionGrid(convertRect(relativePosition, false), startValue, options.ToArray(), 2);
	}


	private float numberField(Rect relativePosition, float startValue) {
		try {
			return float.Parse(GUI.TextField(convertRect(relativePosition, false), startValue.ToString()));
		} catch (System.FormatException) {
			return startValue;
		}
	}
	private int numberField(Rect relativePosition, int startValue) {
		try {
			return int.Parse(GUI.TextField(convertRect(relativePosition, false), startValue.ToString()));
		} catch (System.FormatException) {
			return startValue;
		}
	}

	private void adjustFontSize(GUIStyle style, float height) {
		style.fontSize = (int)(height *Screen.height);
	}

	private Rect convertRect(Rect r, bool fixedHeight) {
		if (fixedHeight)
			return new Rect(r.x * Screen.height, r.y * Screen.height, r.width * Screen.height, r.height);
		return new Rect(r.x * Screen.height, r.y * Screen.height, r.width * Screen.height, r.height * Screen.height);
	}

	private void join() {
		NetworkManager manager = NetworkManager.singleton;
		manager.networkAddress = host;
		manager.StartClient();
		activeAtStart = false;
		Cursor.lockState = CursorLockMode.Locked;
		networkDiscovery.StopBroadcast();
	}

	private void begin() {
	    activeAtStart = false;
		string activeScenePath = SceneManager.GetActiveScene().path;
		SceneData? sceneData = SceneCatalog.sceneCatalog.getSceneData(activeScenePath);
	    if (sceneData == null || !sceneData.Value.supportedGameModes.Contains(serverConfig.gameMode)) {
	        List<SceneData> scenes = SceneCatalog.sceneCatalog.getScenesForGameMode(serverConfig.gameMode);
	        if (scenes.Count == 0) {
	            return;
	        }
	        SceneLoader.sceneLoader.loadScene(scenes[0].path, this);
	    } else if (sceneData != null && sceneData.Value.supportedGameModes.Contains(serverConfig.gameMode)) {
            startGame();
	    }
	}

	private void quit() {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void onSceneLoaded() {
        SceneCatalog sceneCatalog = SceneCatalog.sceneCatalog;
        SceneData? sceneData = sceneCatalog.getSceneData(SceneManager.GetActiveScene().path);
        if (sceneData != null)
            menu.serverConfig = sceneData.Value.configuration;
        GlobalConfig.globalConfig.configuration = serverConfig;
        startGame();

    }

    private void startGame() {
        NetworkManager manager = NetworkManager.singleton;
        manager.StartHost();
        //player.gameObject.SetActive(true);
        //GetComponent<Camera>().enabled = false;
        //GetComponent<AudioListener>().enabled = false;
        menuHistory.Clear();
        activeAtStart = false;
        Cursor.lockState = CursorLockMode.Locked;
        networkDiscovery.Initialize();
        networkDiscovery.broadcastData = serverName;
        networkDiscovery.StartAsServer();
    }
}
