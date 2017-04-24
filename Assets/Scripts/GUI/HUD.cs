using System.Collections.Generic;
using UnityEngine;

public class HUD : MonoBehaviour {

	public static HUD hud;
	
	public Fireflies.Config fireflyConfig;

	private Dictionary<string, Fireflies> elements = new Dictionary<string, Fireflies>();
	private bool showRobots;

	public void Start() {
		hud = this;
	}

	public void Update() {
		List<string> staleElements = new List<string>();
		foreach (string element in elements.Keys) {
			if (elements[element].isClear())
				staleElements.Add(element);
			else
				elements[element].Update();
		}

		foreach (string element in staleElements) {
			elements.Remove(element);
		}
	}

	public void OnGUI() {
		Vector2 offset = new Vector2(Screen.width /2, Screen.height /2);
		foreach (Fireflies flies in elements.Values) {
			flies.OnGUI(offset, Screen.height);
		}
	}

	public void setFireflyElement(string elementName, List<Vector2> positions, bool shuffle=true) {
		if (!elements.ContainsKey(elementName))
			elements[elementName] = new Fireflies(fireflyConfig);
		elements[elementName].setPositions(positions, shuffle);
	}

	public bool clearFireflyElement(string elementName) {
		if (!elements.ContainsKey(elementName))
			return false;
		return elements[elementName].unassign();
	}
}
