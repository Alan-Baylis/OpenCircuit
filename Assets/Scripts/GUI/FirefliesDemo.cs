using System.Collections.Generic;
using UnityEngine;

public class FirefliesDemo : MonoBehaviour {

	public string text;
	public float fontSize = 6;
	public bool shuffleOnChange = true;
	public Fireflies.Config fireflyConfig;

	private Fireflies fireflies = new Fireflies();
	private string lastText = "";

	// Update is called once per frame
	void Update () {
		fireflies.config = fireflyConfig;

		if (text != lastText) {
			fireflies.setPositions(FireflyFont.getString(text, fontSize, Vector2.zero), shuffleOnChange);
			lastText = text;
		}
		fireflies.Update();
	}

	public void OnGUI() {
		fireflies.OnGUI(new Vector2(Screen.width /2, 100));
	}

	private Rect centeredRect(Vector2 position, Vector2 size) {
		return new Rect(position - size / 2, size);
	}

	protected class Firefly {
		public Vector2 position, velocity;
		public int letterIndex, positionIndex;
	}
}
