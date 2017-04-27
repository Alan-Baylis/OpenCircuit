using UnityEngine;

public class MessageDialog : AbstractDialogBox {

	public int fontSize = 30;
	public string message;

	protected override void OnGUI() {
		int width = 400;
		int height = 50;
		int buttonFontSize = menu.skin.button.fontSize;
		menu.skin.button.fontSize = fontSize;
		Rect position = new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height);
		GUI.DrawTexture(position, Menu.menu.background);
		TextAnchor textAlignment = menu.skin.button.alignment;
		menu.skin.button.alignment = TextAnchor.MiddleCenter;
		GUI.Label(position, message, Menu.menu.skin.button);
		menu.skin.button.alignment = textAlignment;
		menu.skin.button.fontSize = buttonFontSize;
	}
}
