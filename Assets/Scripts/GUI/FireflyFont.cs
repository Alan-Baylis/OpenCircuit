using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyFont {

	public static List<Vector2> getString(string s, float fontSize, Vector2 offset, bool center=false) {
		List<Vector2> positions = new List<Vector2>();
		offset.x += center ? -(s.Length *letterWidth + (s.Length - 1) *letterGap) *0.5f *fontSize : 0;
		foreach (char c in s) {
			getChar(c, positions, offset, fontSize);
			offset.x += (letterWidth + letterGap) *fontSize;
		}
		return positions;
	}

	public static List<Vector2> getChar(char c, Vector2 offset, float multiplier) {
		List<Vector2> positions = new List<Vector2>();
		getChar(c, positions, offset, multiplier);
		return positions;
	}

	public static void getChar(char c, List<Vector2> positions, Vector2 offset, float multiplier) {
		if (!characters.ContainsKey(c)) {
			positions.Add(offset);
		} else {
			foreach (Vector2 position in characters[c]) {
				positions.Add(position *multiplier +offset);
			}
		}
	}

	private FireflyFont() {}

	private const int letterWidth = 5;
	private const int letterHeight = 9; // 7 for primary, 2 for hanging letters
	private const float letterGap = 1;

	private static readonly Dictionary<char, Vector2[]> characters = new Dictionary<char, Vector2[]> {
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
