using UnityEngine;
using UnityEngine.Networking;

public class Spectator : NetworkBehaviour {

	private Menu menu;
	private bool playerControlsEnabled = true;

	[SyncVar]
	public NetworkInstanceId clientControllerId;
	private ClientController myClientController;

	private int? selectedScore;

	public ClientController clientController {
		get {
			if (myClientController == null)
				myClientController = ClientScene.FindLocalObject(clientControllerId).GetComponent<ClientController>();
			return myClientController;
		}
	}

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

		if (clientController.admin && GlobalConfig.globalConfig.cameraManager.getSceneCamera().enabled) {
			if (selectedScore != null) {
				HUD.hud.setFireflyElement("deleteMarker", this,
					FireflyFont.getString("-", .2f, new Vector2(0f, -.3f + selectedScore.Value * .1f)));
			} else {
				HUD.hud.clearFireflyElement("deleteMarker");
			}


			if (Input.GetButtonDown("Vertical")) {
				if (GlobalConfig.globalConfig.leaderboard.getScoreCount() > 0) {
					if (selectedScore == null) {
						selectedScore = 0;
					} else if (Input.GetAxis("Vertical") > 0) {
						decrementSelected();
					} else {
						incrementSelected();
					}
				} else {
					selectedScore = null;
				}
			}

			if (Input.GetButtonDown("Delete") && selectedScore != null) {
				GlobalConfig.globalConfig.leaderboard.removeScore(selectedScore.Value);
			}
		}
	}

	private void incrementSelected() {
		if (selectedScore >= GlobalConfig.globalConfig.leaderboard.getScoreCount() - 1) {
			selectedScore = 0;
		} else {
			++selectedScore;
		}
	}

	private void decrementSelected() {
		if (selectedScore == 0) {
			selectedScore = GlobalConfig.globalConfig.leaderboard.getScoreCount() - 1;
		} else {
			--selectedScore;
		}
	}
}
