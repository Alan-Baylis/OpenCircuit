using UnityEngine;
using UnityEngine.Networking;

public class Spectator : NetworkBehaviour {

	private Menu menu;
	private bool playerControlsEnabled = true;


	// Use this for initialization
	void Start () {
		menu = Menu.menu;
	}

	[ClientCallback]
	void Update() {
		if(!isLocalPlayer) {
			return;
		}
		/****************MENU****************/
		if (Input.GetButtonDown("Menu")) {
			menu.toggleInGameMenu();
		}

		if (menu.paused())
			return;

		if (Input.GetButtonDown("Use")) {
			GlobalConfig.globalConfig.cameraManager.switchCamera();
		}
	}
}
