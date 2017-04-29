using UnityEngine;
using UnityEngine.Networking;

[AddComponentMenu("Scripts/Player/Controls")]
public class Controls : NetworkBehaviour {

	private Player myPlayer;
	private bool playerControlsEnabled = true;
	private Menu menu;

	public float mouseSensitivity = 1;
	public float zoomingSensitivity = 0.8f;
	public bool invertLook = false;
	public bool enableMousePadHacking = false;


	private ControlStatus status;


	public override int GetNetworkChannel() {
		return 0;
	}

	void Awake () {
		myPlayer = GetComponent<Player> ();
		menu = GameObject.FindGameObjectWithTag("Menu").GetComponent<Menu>();
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

		if (Time.timeScale == 0 || !playerControlsEnabled)
			return;

		/****************MOVEMENT****************/
        if (updateStatus(ref status.forward, Input.GetAxis("Vertical"), 0)) {
			myPlayer.mover.setForward(status.forward);
			CmdSetForward(status.forward);
		}
			
		if (updateStatus(ref status.right, Input.GetAxis("Horizontal"), 0)) {
			myPlayer.mover.setRight(status.right);
			CmdSetRight(status.right);
		}

        if (hasControls() && Input.GetButtonDown("Jump")) {
            if (!isServer)
                myPlayer.mover.jump();
            CmdJump();
        }
			
		if (updateStatus(ref status.sprinting, Input.GetButton("Sprint"), status.sprinting)) {
			setSprinting(status.sprinting);
			CmdSetSprint(status.sprinting);
		}
			
		if (updateStatus(ref status.crouching, Input.GetButton("Crouch"), status.crouching)) {
			myPlayer.mover.setCrouching(status.crouching);
			CmdSetCrouch(status.crouching);
		}

		if (hasControls() && Input.GetButton("Score")) {
			GlobalConfig.globalConfig.scoreboard.enabled = true;
		} else {
			GlobalConfig.globalConfig.scoreboard.enabled = false;
		}

		if(hasControls() && Input.GetButtonDown("Reload")) {
			myPlayer.inventory.reloadEquipped();
			if (!isServer)
				CmdReloadEquipped();
		}

		if (hasControls()) {
			float sensitivityMult = mouseSensitivity * (myPlayer.zooming ? zoomingSensitivity : 1);
			float hori = Input.GetAxis("Look Horizontal") * sensitivityMult;
			float verti = Input.GetAxis("Look Vertical") * sensitivityMult;
			if (invertLook)
				verti = -verti;
			myPlayer.looker.rotate(hori, verti);
		}
		
		/****************ACTION****************/
		if (updateStatus(ref status.useEquipment, Input.GetButton("Use"), false)) {
			if (status.useEquipment) {
				myPlayer.inventory.useEquipped();
			} else {
				myPlayer.inventory.stopUsingEquiped();
			}
		}

		if (updateStatus(ref status.zoom, Input.GetButton("Zoom"), false)) {
			myPlayer.zooming = status.zoom;
			if (!isServer)
				CmdSetZooming(status.zoom);
		}

		if (hasControls() && Input.GetButtonDown("Interact")) {
			CmdInteract();
		}

		if (hasControls() && Input.GetButtonDown("Build")) {
			if (!myPlayer.inventory.inContext()) {
				myPlayer.inventory.pushContext(typeof(BuildTool));
			} else {
				myPlayer.inventory.popContext(typeof(BuildTool));
			}
		}


	}

	[Command]
	protected void CmdInteract() {
		myPlayer.interactor.interact();
	}

	[Command]
	protected void CmdSetRight(float amount) {
		myPlayer.mover.setRight(amount);
	}

	[Command]
	protected void CmdSetForward(float amount) {
		myPlayer.mover.setForward(amount);
	}

	[Command]
	protected void CmdJump() {
		myPlayer.mover.jump();
	}

	[Command]
	protected void CmdSetCrouch(bool crouch) {
		myPlayer.mover.setCrouching(crouch);
	}

	[Command]
	protected void CmdSetSprint(bool sprint) {
		setSprinting(sprint);
	}

	protected void setSprinting(bool sprint) {
		myPlayer.mover.setSprinting(sprint);
		myPlayer.inventory.setSprinting(sprint);
	}

	[Command]
	protected void CmdSetZooming(bool zooming) {
		myPlayer.zooming = zooming;
	}

	[Command]
	protected void CmdReloadEquipped() {
		myPlayer.inventory.reloadEquipped();
	}

	public void disablePlayerControls() {
		playerControlsEnabled = false;
		myPlayer.mover.setForward(0);
		myPlayer.mover.setRight(0);
	}
	
	public void enablePlayerControls() {
		playerControlsEnabled = true;
	}

	private bool hasControls() {
		return !menu.paused() && !myPlayer.frozen;
	}

	public bool updateStatus<T>(ref T currentValue, T newValue, T defaultValue) {
		if (!hasControls())
			newValue = defaultValue;
		bool changed = !currentValue.Equals(newValue);
		currentValue = newValue;
		return changed;
	}

	private struct ControlStatus {
		public float forward;
		public float right;
		public bool sprinting;
		public bool crouching;
		public bool useEquipment;
		public bool zoom;
	}
}
