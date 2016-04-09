 using UnityEngine;
using System.Collections;

public class TooltipTrigger : MonoBehaviour {

	public string tooltip;
	public float duration = 10;
	public float maxOpacity = 0.8f;
	public float fadeInRate = 1f;
	public float fadeOutRate = 0.2f;

	private bool shown = false;
	private float opacity = 0;
	private float endTime = -1;

	public void OnTriggerEnter(Collider col) {
		if (shown)
			return;

		Player player = col.GetComponent<Player>();
		if (player != null && player.isLocalPlayer) {
			shown = true;
			endTime = Time.time + duration;
			StartCoroutine("showTooltip");
		}
	}

	public IEnumerator showTooltip() {
		while (true) {
			if (Time.time < endTime) {
				if (opacity < maxOpacity)
					opacity = Mathf.Min(opacity + fadeInRate * maxOpacity * Time.deltaTime, maxOpacity);
			} else if (opacity > 0) {
				opacity = Mathf.Max(opacity - fadeOutRate * maxOpacity * Time.deltaTime, 0);
			} else {
				break;
			}
			yield return new WaitForEndOfFrame();
		}
	}

	public void OnGUI() {
		if (opacity <= 0)
			return;

		Color lastColor = GUI.color;
		GUI.color = new Color(1, 1, 1, opacity);
		GUI.Label(new Rect(Screen.width /4, Screen.height /4, 200, 300), tooltip);
		GUI.color = lastColor;
	}
}
