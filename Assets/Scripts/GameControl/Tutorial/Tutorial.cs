using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour {

	public List<Message> messages = new List<Message>();
	public MessageDialog dialogPrefab;

	private Player player;
	private MessageDialog dialog;
	private int currentMessageIndex;
	private Message ? currentMessage;

	private float timer;

	void Update() {
		if (currentMessage != null && currentMessage.Value.clickToContinue && Input.GetButtonDown("Use")) {
			player.frozen = false;
			nextMessage();
		}

		if (currentMessage != null && currentMessage.Value.timerToContinue) {
			timer += Time.deltaTime;
			if (timer > currentMessage.Value.timer) {
				if (currentMessage.Value.closeOnTimeout) {
					dialog.gameObject.SetActive(false);
					currentMessage = null;
				} else {
					nextMessage();
				}
			}
		}
	}

	public void startPlayer(Player player) {
		if (player.isLocalPlayer && this.player != player) {
			this.player = player;
			player.frozen = true;
			nextMessage();
		}
	}

	public void nextMessage() {
		timer = 0;
		if (dialog == null) {
			currentMessage = messages[currentMessageIndex];
			dialog = Instantiate(dialogPrefab);
			dialog.fontSize = 20;
			dialog.message = messages[currentMessageIndex].message.Replace ("\\n", "\n");
			dialog.size = new Vector2(450, 150);
			dialog.posOffset = new Vector2(0, -200);
			++currentMessageIndex;
		} else if (currentMessageIndex < messages.Count) {
			currentMessage = messages[currentMessageIndex];
			dialog.message = messages[currentMessageIndex].message.Replace ("\\n", "\n");
			dialog.gameObject.SetActive(true);
			++currentMessageIndex;
		}
	}

	[System.Serializable]
	public struct Message {
		public bool clickToContinue;
		public bool timerToContinue;
		public bool closeOnTimeout;
		public float timer;
		public string message;
	}
}
