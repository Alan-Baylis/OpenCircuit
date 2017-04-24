using System.Collections.Generic;
using UnityEngine;

public class HUD : MonoBehaviour {

	public static HUD hud;
	
	public Fireflies.Config fireflyConfig;

	private Dictionary<string, Element> elements = new Dictionary<string, Element>();
	private bool showRobots;

	public void Start() {
		hud = this;
	}

	public void Update() {
		List<string> staleElements = new List<string>();
		foreach (string name in elements.Keys) {
			Element e = elements[name];
			if (e.flies.isClear()) {
				staleElements.Add(name);
			} else {
				if (e.owner == null && Time.time >= e.expiration && e.flies.isAssigned())
					e.flies.unassign();
				e.flies.Update();
			}
		}

		foreach (string element in staleElements) {
			elements.Remove(element);
		}
	}

	public void OnGUI() {
		Vector2 offset = new Vector2(Screen.width /2, Screen.height /2);
		foreach (Element e in elements.Values) {
			e.flies.OnGUI(offset, Screen.height);
		}
	}

	public void setFireflyElement(string elementName, UnityEngine.Object owner, List<Vector2> positions, bool shuffle=true) {
		if (!elements.ContainsKey(elementName))
			elements[elementName] = new Element(fireflyConfig);
		Element e = elements[elementName];
		e.flies.setPositions(positions, shuffle);
		e.owner = owner;
		e.expiration = 0;
	}

	public void setFireflyElement(string elementName, float lifetime, List<Vector2> positions, bool shuffle=true) {
		if (!elements.ContainsKey(elementName))
			elements[elementName] = new Element(fireflyConfig);
		Element e = elements[elementName];
		e.flies.setPositions(positions, shuffle);
		e.owner = null;
		e.expiration = Time.time +lifetime;
	}

	public bool clearFireflyElement(string elementName) {
		if (!elements.ContainsKey(elementName))
			return false;
		return elements[elementName].flies.unassign();
	}

	public class Element {
		public Fireflies flies;
		public UnityEngine.Object owner;
		public float expiration;

		public Element(Fireflies.Config config) {
			flies = new Fireflies(config);
		}
	}
}
