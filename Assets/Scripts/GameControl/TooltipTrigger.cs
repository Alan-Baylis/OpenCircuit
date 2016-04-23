 using UnityEngine;
using System.Collections;

public class TooltipTrigger : MonoBehaviour {

	public string tooltip;
	//public float duration = 10;
	public float maxOpacity = 0.8f;
	public float fadeInRate = 1f;
	public float fadeOutRate = 0.2f;

	public GUISkin skin;

	private bool shown = false;
	private float opacity = 0;
	private float endTime = -1;
	private bool triggered = false;

	public void OnTriggerEnter(Collider col) {
		if (shown)
			return;

		Player player = col.GetComponent<Player>();
		if (player != null && player.isLocalPlayer) {
			shown = true;
			//endTime = Time.time + duration;
			StartCoroutine("showTooltip");
			if(!triggered) {
				GetComponent<AudioSource>().Play();
			}
			triggered = true;

		}
	}

	public void OnTriggerExit(Collider col) {
		if(!shown)
			return;

		Player player = col.GetComponent<Player>();
		if(player != null && player.isLocalPlayer) {
			shown = false;
		}
	}

	public IEnumerator showTooltip() {
		while (true) {
			if (shown) {
				if (opacity < maxOpacity)
					opacity = Mathf.Min(opacity + fadeInRate * maxOpacity * Time.deltaTime, maxOpacity);
			} else if (opacity > 0) {
				opacity = Mathf.Max(opacity - fadeOutRate * maxOpacity * Time.deltaTime, 0);
			} else {
				break;
			}
			yield return null;
		}
	}

	public void OnGUI() {
		if (opacity <= 0)
			return;

		GUI.skin = skin;
		Color lastColor = GUI.color;
		GUI.color = new Color(1, 1, 1, opacity);
		GUI.Label(new Rect(Screen.width /6, Screen.height /6, 400, 300), tooltip);
		GUI.color = lastColor;
	}
}
