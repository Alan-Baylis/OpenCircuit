using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[AddComponentMenu("Scripts/Player/Controls")]
public class Controls : NetworkBehaviour {

	private Player myPlayer;
	private bool playerControlsEnabled = true;
	private Menu menu;

	public float mouseSensitivity = 1;
	public bool invertLook = false;
	public bool enableMousePadHacking = false;


	public override int GetNetworkChannel() {
		return 0;
	}

	void Awake () {
		myPlayer = this.GetComponent<Player> ();
		menu = GameObject.FindGameObjectWithTag("Menu").GetComponent<Menu>();
	}

	void Update () {
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
		float amount = Input.GetAxis("Vertical");
		myPlayer.mover.setForward(amount);
		CmdSetForward(amount);

		amount = Input.GetAxis("Horizontal");
		myPlayer.mover.setRight(amount);
		CmdSetRight(amount);



		if (Input.GetButtonDown("Jump")) {
			if (!isServer)
				myPlayer.mover.jump();
			CmdJump();
		}

		bool sprinting = Input.GetButton("Sprint");
		setSprinting(sprinting);
        CmdSetSprint(sprinting);

		bool crouching = Input.GetButton("Crouch");
		myPlayer.mover.setCrouching(crouching);
        CmdSetCrouch(crouching);

		/****************INVENTORY***************/
		if (!myPlayer.inventory.inContext()) {
			if (enableMousePadHacking) {
				if (Input.GetButtonDown("Equip1")) {
					if (myPlayer.inventory.isSelecting())
						myPlayer.inventory.doSelect(-1);
					else
						myPlayer.inventory.doSelect(0);
				} else if (Input.GetButtonDown("Equip2")) {
					if (myPlayer.inventory.isSelecting())
						myPlayer.inventory.doSelect(-1);
					else
						myPlayer.inventory.doSelect(1);
				} else if (Input.GetButtonDown("Equip3")) {
					if (myPlayer.inventory.isSelecting())
						myPlayer.inventory.doSelect(-1);
					else
						myPlayer.inventory.doSelect(2);
				}
			} else {
				if (Input.GetButton("Equip1")) {
					myPlayer.inventory.doSelect(0);
				} else if (Input.GetButton("Equip2")) {
					myPlayer.inventory.doSelect(1);
				} else if (Input.GetButton("Equip3")) {
					myPlayer.inventory.doSelect(2);
				} else {
					myPlayer.inventory.doSelect(-1);
				}
			}
		}

		// nothing after this point is done while in menu
		if(menu.paused()) {
			return;
		}

		if (myPlayer.inventory.isSelecting()) {
			myPlayer.inventory.moveMouse(new Vector2(Input.GetAxis("Look Horizontal"), Input.GetAxis("Look Vertical")));
		} else {
			if (invertLook)
				myPlayer.looker.rotate(Input.GetAxis("Look Horizontal") * mouseSensitivity, -Input.GetAxis("Look Vertical") * mouseSensitivity);
			else
				myPlayer.looker.rotate(Input.GetAxis("Look Horizontal") * mouseSensitivity, Input.GetAxis("Look Vertical") * mouseSensitivity);
		}

		/****************ACTION****************/

		if (Input.GetButtonDown("Use")) {
			myPlayer.inventory.useEquipped();
			CmdUseEquipped();
		}
		if (Input.GetButtonUp("Use")) {
			myPlayer.inventory.stopUsingEquiped();
			CmdStopUsingEquipped();
		}
		if (Input.GetButtonDown ("Interact")) {
			CmdInteract();
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
		//print("Forward: " +amount);
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
	protected void CmdUseEquipped() {
		myPlayer.inventory.useEquipped();
	}

	[Command]
	protected void CmdStopUsingEquipped() {
		myPlayer.inventory.stopUsingEquiped();
	}

	public void disablePlayerControls() {
		playerControlsEnabled = false;
		myPlayer.mover.setForward(0);
		myPlayer.mover.setRight(0);
	}
	
	public void enablePlayerControls() {
		playerControlsEnabled = true;
	}
}
