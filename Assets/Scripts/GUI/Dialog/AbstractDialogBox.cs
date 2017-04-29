using UnityEngine;

public abstract class AbstractDialogBox : MonoBehaviour {

	protected Menu menu;

	// Use this for initialization
	void Start () {
		menu = Menu.menu;
		menu.pause();
	}

	protected abstract void OnGUI();
}
