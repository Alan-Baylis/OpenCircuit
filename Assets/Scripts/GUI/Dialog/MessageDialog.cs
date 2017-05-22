using UnityEngine;

public class MessageDialog : AbstractDialogBox {

	public int fontSize = 30;
	public string message;
	public Vector2 posOffset = new Vector2(0, 0);
	public Vector2 size = new Vector2(400, 50);

	protected override void OnGUI() {
		int buttonFontSize = menu.skin.button.fontSize;
		float buttonFixedWidth = menu.skin.button.fixedWidth;
		float buttonFixedHeight = menu.skin.button.fixedHeight;

		menu.skin.button.fontSize = fontSize;
		menu.skin.button.fixedWidth = size.x;
		menu.skin.button.fixedHeight = size.y;

		Rect position = new Rect((Screen.width - size.x) / 2 + posOffset.x, (Screen.height - size.y) / 2 + posOffset.y, size.x, size.y);
		GUI.DrawTexture(position, Menu.menu.background);
		TextAnchor textAlignment = menu.skin.button.alignment;
		menu.skin.button.alignment = TextAnchor.MiddleCenter;
		GUI.Label(position, message, Menu.menu.skin.button);
		menu.skin.button.alignment = textAlignment;

		menu.skin.button.fontSize = buttonFontSize;
		menu.skin.button.fixedWidth = buttonFixedWidth;
		menu.skin.button.fixedHeight = buttonFixedHeight;
	}
}
