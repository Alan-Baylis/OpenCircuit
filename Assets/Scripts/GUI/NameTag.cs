using UnityEngine;
using UnityEngine.Networking;

public class NameTag : NetworkBehaviour {

	[SyncVar(hook = "onSetName")]
	public string displayName;

	public override void OnStartClient() {
		base.OnStartClient();
		if (!string.IsNullOrEmpty(displayName)) {
			onSetName(displayName);
		}
	}

	[ClientCallback]
	void OnGUI() {
		Camera cam = Camera.current;
		Vector3 pos;
		if(cam != null) {
			Vector3 worldTextPos = transform.position + new Vector3(0, 1, 0);
			pos = cam.WorldToScreenPoint(worldTextPos);
			if(Vector3.Dot(cam.transform.forward, (worldTextPos - cam.transform.position).normalized) < 0) {
				return;
			}
		} else {
			return;
		}


		Font font =	Resources.Load<Font>("Fonts/Courier.ttf");
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.font = font;
		style.fontSize = 14;
		Vector2 labelSize = style.CalcSize(new GUIContent(displayName));

		Rect textCentered = new Rect(pos.x - labelSize.x / 2, Screen.height - pos.y - labelSize.y + 4, labelSize.x, labelSize.y);
		GUI.Label(textCentered, displayName, style);
	}

	private void onSetName(string name) {
		displayName = name;
		if (!enabled && !GetComponent<Player>().clientController.isLocalPlayer)
			enabled = true;
	}
}
