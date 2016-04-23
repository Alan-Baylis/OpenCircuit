using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Scripts/Menu/Menu")]
public class Menu : MonoBehaviour {

	private Rect startRect = new Rect(-0.03f, 0.15f, 0.4f, 0.1f);
	private Rect exitRect = new Rect(-0.035f, 0.5f, 0.4f, 0.1f);
	private Rect optionsRect = new Rect(-0.01f, 0.3f, 0.4f, 0.1f);
	private Rect loadRect = new Rect(0.1f, 0.5f, 0.4f, 0.1f);
	private Rect backRect = new Rect(0.1f, 0.7f, 0.4f, 0.1f);
	private Rect titleRect = new Rect(-.25f, 0f, 1.2f, 0.2f);
	private Rect resumeRect = new Rect(-0.01f, .15f, .4f, .1f);
	private state currentMenu = state.MainMenu;
	private Stack<state> menuHistory = new Stack<state>();
	private static bool didWin = false;

	public float defaultScreenHeight = 1080;
	public bool activeAtStart = true;
	public GUISkin skin;
	public Texture2D background;
	//public Texture2D controls;
	public Vector3 endCamPosition;
	public Vector3 endCamRotation;

	public static Player player {
		get { return GameObject.FindGameObjectWithTag("Player").GetComponent<Player>(); }
	}

	private static Menu myMenu = null;
	public static Menu menu { get {
			if (myMenu == null)
				myMenu = GameObject.FindGameObjectWithTag("Menu").GetComponent<Menu>();
			return myMenu;
	}}
	
	private enum state {
		MainMenu, InGameMenu, Options, Load, Win, Lose
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
		//myPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
		//if (activeAtStart && !didWin) {
		//	myPlayer.gameObject.SetActive(false);
		//} else {
		//	begin();
		//}
		pause();
		currentMenu = state.MainMenu;
		didWin = false;
	}

	public void OnGUI() {
		if (!activeAtStart) return;
		GUI.depth = -1;
		GUI.skin = skin;
		float width = (Screen.height * background.width) / background.height;
		GUI.DrawTexture(new Rect(-250, 0, width, Screen.height), background);
		adjustFontSize(skin.button, titleRect.height);
		GUI.Label(convertRect(titleRect, false), "Guns 'n' Robots", skin.button);
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
		didWin = true;
		GetComponent<Camera>().enabled = true;
		GetComponent<AudioListener>().enabled = true;
		player.gameObject.SetActive(false);
		transform.position = endCamPosition;
		transform.eulerAngles = endCamRotation;
	}

	public void lose() {
		pause();
		currentMenu = state.Lose;
	}

	private void doLose() {
		//adjustFontSize(skin.button, startRect.height);
		//if (GUI.Button(convertRect(startRect, false), "Restart", skin.button)) {
		//	currentMenu = state.MainMenu;
		//	SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		//}
		adjustFontSize(skin.button, exitRect.height);
		if (GUI.Button(convertRect(exitRect, false), "Quit", skin.button)) {
            quit();
		}
		int width = 200;
		int height = 50;
		Rect position = new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height);
		GUI.Label(position, "You Lost!", skin.button);
	}

	private void doWin() {
		//adjustFontSize(skin.button, startRect.height);
		//if (GUI.Button(convertRect(startRect, false), "Play Again", skin.button)) {
		//	currentMenu = state.MainMenu;
		//	SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		//}
		//adjustFontSize(skin.button, optionsRect.height);
		//if (GUI.Button(convertRect(optionsRect,false), "Options", skin.button)) {
		//	menuHistory.Push(currentMenu);
		//	currentMenu = state.Options;
		//}
		adjustFontSize(skin.button, exitRect.height);
		if (GUI.Button(convertRect(exitRect, false), "Quit", skin.button)) {
            quit();
		}
		int width = 200;
		int height = 50;
		Rect position = new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height);
		GUI.Label(position, "You Won!", skin.button);
	}

	private void doInGameMenu() {
		adjustFontSize(skin.button, resumeRect.height);
		if (GUI.Button(convertRect(resumeRect, false), "Resume", skin.button)) {
			toggleInGameMenu();
		}
		//adjustFontSize(skin.button, loadRect.height);
		//if (GUI.Button(convertRect(loadRect, false), "Restart Game", skin.button)) {
		//	currentMenu = state.MainMenu;
		//	SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		//}
		adjustFontSize(skin.button, exitRect.height);
		if (GUI.Button(convertRect(exitRect, false), "Quit", skin.button)) {
            quit();
		}
		adjustFontSize(skin.button, optionsRect.height);
		if (GUI.Button(convertRect(optionsRect, false), "Options", skin.button)) {
			menuHistory.Push(currentMenu);
			currentMenu = state.Options;
		}
	}

	private void doMainMenu() {
		adjustFontSize(skin.button, startRect.height);
		if (GUI.Button(convertRect(startRect, false), "Begin", skin.button)) {
			begin();
		}
		adjustFontSize(skin.button, exitRect.height);
		if (GUI.Button(convertRect(exitRect, false), "Quit", skin.button)) {
            quit();
		}
		adjustFontSize(skin.button, optionsRect.height);
		if (GUI.Button(convertRect(optionsRect, false), "Options", skin.button)) {
			menuHistory.Push(currentMenu);
			currentMenu = state.Options;
		}
	}

	private void doOptions() {

		// graphics settings

		adjustFontSize(skin.label, 0.07f);
		GUI.Label(convertRect(new Rect(0.05f, 0.1f, 0.4f, 0.07f), false), "Shadow Distance:  " +QualitySettings.shadowDistance.ToString("##,0.") +" m");
		QualitySettings.shadowDistance = GUI.HorizontalSlider(convertRect(new Rect(0.05f, 0.16f, 0.25f, 0.04f), false), QualitySettings.shadowDistance, 0, 200);
		string[] vSyncOptions = { "None", "Full", "Half" };
		adjustFontSize(skin.label, 0.07f);
		GUI.Label(convertRect(new Rect(0.05f, 0.18f, 0.2f, 0.07f), false), "VSync: " +vSyncOptions[QualitySettings.vSyncCount]);
		//QualitySettings.vSyncCount = GUI.SelectionGrid(convertRect(new Rect(0.1f, 0.2f, 0.3f, 0.04f)), QualitySettings.vSyncCount, vSyncOptions, 3);
		QualitySettings.vSyncCount = (int)(GUI.HorizontalSlider(convertRect(new Rect(0.05f, 0.24f, 0.2f, 0.04f), false), QualitySettings.vSyncCount, 0, 2) +0.5f);

		// input settings
		Player myPlayer = player;
        adjustFontSize(skin.label, 0.07f);
		GUI.Label(convertRect(new Rect(0.05f, 0.28f, 0.4f, 0.07f), false), "Look Sensitivity:  " +(myPlayer.controls.mouseSensitivity *4).ToString("##,0.0#"));
		myPlayer.controls.mouseSensitivity = GUI.HorizontalSlider(convertRect(new Rect(0.05f, 0.34f, 0.25f, 0.04f), false), myPlayer.controls.mouseSensitivity, 0.0625f, 2);
		myPlayer.controls.mouseSensitivity = ((int)(myPlayer.controls.mouseSensitivity * 16 + 0.5f)) / 16f;
		GUI.Label(convertRect(new Rect(0.05f, 0.36f, 0.4f, 0.07f), false), "Look Inversion: ");
		myPlayer.controls.invertLook = GUI.Toggle(convertRect(new Rect(0.25f, 0.38f, 0.25f, 0.07f), false), myPlayer.controls.invertLook, "");

		//GUI.DrawTexture(convertRect(new Rect(0.03f, 0.41f, 0.3f, 0.3f), false), controls);

		// back button
		adjustFontSize(skin.button, backRect.height);
		if (GUI.Button(convertRect(backRect, false), "Back", skin.button)) {
			currentMenu = menuHistory.Pop();
		}
	}

	private void adjustFontSize(GUIStyle style, float height) {
		style.fontSize = (int)(height *Screen.height *0.5f);
	}

	private Rect convertRect(Rect r, bool fixedHeight) {
		if (fixedHeight)
			return new Rect(r.x * Screen.height, r.y * Screen.height, r.width * Screen.height, r.height);
		return new Rect(r.x * Screen.height, r.y * Screen.height, r.width * Screen.height, r.height * Screen.height);
	}

	private void begin() {
		NetworkManager manager = NetworkManager.singleton;
		manager.StartHost();
		//player.gameObject.SetActive(true);
		//GetComponent<Camera>().enabled = false;
		//GetComponent<AudioListener>().enabled = false;
		menuHistory.Clear();
		activeAtStart = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

    private void quit() {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
