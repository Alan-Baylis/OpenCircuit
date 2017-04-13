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
			List<Vector2> positions = new List<Vector2>();
			int i = 0;
			foreach(char c in text) {
				foreach(Vector2 position in characters[c]) {
					positions.Add((position +new Vector2(i *(letterWidth +1), 0)) *fontSize);
				}
				++i;
			}
			fireflies.setPositions(positions, shuffleOnChange);
			lastText = text;
		}
		fireflies.Update();
	}

	public void OnGUI() {
		fireflies.OnGUI(new Vector2(Screen.width /2, 100));
	}

	protected int getIndexCount() {
		int count = 0;
		foreach(char letter in text) {
			count += characters[letter].Length;
		}
		return count;
	}

	private Rect centeredRect(Vector2 position, Vector2 size) {
		return new Rect(position - size / 2, size);
	}

	protected class Firefly {
		public Vector2 position, velocity;
		public int letterIndex, positionIndex;
	}


	private int letterWidth = 5;
	private int letterHeight = 9; // 7 for primary, 2 for hanging letters

	private Dictionary<char, Vector2[]> characters = new Dictionary<char, Vector2[]> {
		{'0', new Vector2[] {
				new Vector2(1, 0),
				new Vector2(2, 0),
				new Vector2(3, 0),
				new Vector2(4, 1),
				new Vector2(4, 2),
				new Vector2(4, 3),
				new Vector2(4, 4),
				new Vector2(4, 5),
				new Vector2(3, 6),
				new Vector2(2, 6),
				new Vector2(1, 6),
				new Vector2(0, 5),
				new Vector2(0, 4),
				new Vector2(0, 3),
				new Vector2(0, 2),
				new Vector2(0, 1)
			}},
		{'1', new Vector2[] {
				new Vector2(2, 0),
				new Vector2(2, 1),
				new Vector2(2, 2),
				new Vector2(2, 3),
				new Vector2(2, 4),
				new Vector2(2, 5),
				new Vector2(2, 6),
				new Vector2(1, 1),
				new Vector2(1, 6),
				new Vector2(3, 6),
			}},
		{'2', new Vector2[] {
				new Vector2(0, 1),
				new Vector2(1, 0),
				new Vector2(2, 0),
				new Vector2(3, 0),
				new Vector2(4, 1),
				new Vector2(3, 2),
				new Vector2(2, 3),
				new Vector2(1, 4),
				new Vector2(0, 5),
				new Vector2(0, 6),
				new Vector2(1, 6),
				new Vector2(2, 6),
				new Vector2(3, 6),
				new Vector2(4, 6),
			}},
		{'3', new Vector2[] {
				new Vector2(0, 1),
				new Vector2(1, 0),
				new Vector2(2, 0),
				new Vector2(3, 0),
				new Vector2(4, 1),
				new Vector2(4, 2),
				new Vector2(3, 3),
				new Vector2(2, 3),
				new Vector2(4, 4),
				new Vector2(4, 5),
				new Vector2(3, 6),
				new Vector2(2, 6),
				new Vector2(1, 6),
				new Vector2(0, 5),
			}},
		{'4', new Vector2[] {
				new Vector2(4, 4),
				new Vector2(3, 4),
				new Vector2(2, 4),
				new Vector2(1, 4),
				new Vector2(0, 4),
				new Vector2(0, 3),
				new Vector2(1, 2),
				new Vector2(2, 1),
				new Vector2(3, 0),
				new Vector2(3, 1),
				new Vector2(3, 2),
				new Vector2(3, 3),
				new Vector2(3, 5),
				new Vector2(3, 6),
			}},
		{'5', new Vector2[] {
				new Vector2(4, 0),
				new Vector2(3, 0),
				new Vector2(2, 0),
				new Vector2(1, 0),
				new Vector2(0, 0),
				new Vector2(0, 1),
				new Vector2(0, 2),
				new Vector2(1, 2),
				new Vector2(2, 2),
				new Vector2(3, 2),
				new Vector2(4, 3),
				new Vector2(4, 4),
				new Vector2(4, 5),
				new Vector2(3, 6),
				new Vector2(2, 6),
				new Vector2(1, 6),
				new Vector2(0, 5),
			}},
		{'6', new Vector2[] {
				new Vector2(4, 0),
				new Vector2(3, 0),
				new Vector2(2, 0),
				new Vector2(1, 0),
				new Vector2(0, 1),
				new Vector2(0, 2),
				new Vector2(0, 3),
				new Vector2(0, 4),
				new Vector2(0, 5),
				new Vector2(1, 6),
				new Vector2(2, 6),
				new Vector2(3, 6),
				new Vector2(4, 5),
				new Vector2(4, 4),
				new Vector2(3, 3),
				new Vector2(2, 3),
				new Vector2(1, 3),
			}},
		{'7', new Vector2[] {
				new Vector2(0, 1),
				new Vector2(0, 0),
				new Vector2(1, 0),
				new Vector2(2, 0),
				new Vector2(3, 0),
				new Vector2(4, 0),
				new Vector2(4, 1),
				new Vector2(3, 2),
				new Vector2(2, 3),
				new Vector2(2, 4),
				new Vector2(2, 5),
				new Vector2(2, 6),
			}},
		{'8', new Vector2[] {
				new Vector2(1, 0),
				new Vector2(2, 0),
				new Vector2(3, 0),
				new Vector2(4, 1),
				new Vector2(4, 2),
				new Vector2(3, 3),
				new Vector2(2, 3),
				new Vector2(4, 4),
				new Vector2(4, 5),
				new Vector2(3, 6),
				new Vector2(2, 6),
				new Vector2(1, 6),
				new Vector2(0, 5),
				new Vector2(0, 4),
				new Vector2(1, 3),
				new Vector2(0, 2),
				new Vector2(0, 1),
			}},
		{'9', new Vector2[] {
				new Vector2(3, 3),
				new Vector2(2, 3),
				new Vector2(1, 3),
				new Vector2(0, 2),
				new Vector2(0, 1),
				new Vector2(1, 0),
				new Vector2(2, 0),
				new Vector2(3, 0),
				new Vector2(4, 1),
				new Vector2(4, 2),
				new Vector2(4, 3),
				new Vector2(4, 4),
				new Vector2(4, 5),
				new Vector2(3, 6),
				new Vector2(2, 6),
				new Vector2(1, 6),
			}},
	};
}
