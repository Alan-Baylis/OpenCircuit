﻿using System.Collections.Generic;
using UnityEngine;

public class FireflyFont {

	public enum HAlign {
		LEFT, CENTER, RIGHT
	}
	public enum VAlign {
		TOP, CENTER, BOTTOM
	}

	public static List<Vector2> getString(string text, float fontSize, Vector2 offset, HAlign hAlign=HAlign.LEFT, VAlign vAlign=VAlign.TOP) {
		List<Vector2> positions = new List<Vector2>();
		string[] lines = text.Split('\n');
		int line = 0;
		foreach (string s in lines) {
			Vector2 lineOffset = offset;
			float sizeMult = fontSize / (letterHeight + lineGap);

			// apply alignment
			if (hAlign == HAlign.CENTER)
				lineOffset.x -= getTextWidth(s, sizeMult) *0.5f;
			else if (hAlign == HAlign.RIGHT)
				lineOffset.x -= getTextWidth(s, sizeMult);
			if (vAlign == VAlign.CENTER)
				lineOffset.y -= getTextHeight(sizeMult) *0.5f *(lines.Length - line);
			else if (vAlign == VAlign.BOTTOM)
				lineOffset.y -= getTextHeight(sizeMult) *(lines.Length - line);
			else
				lineOffset.y += getTextHeight(sizeMult) *line;

			// get positions
			foreach (char c in s) {
				getChar(c, positions, lineOffset, sizeMult);
				lineOffset.x += (letterWidth + letterGap) * sizeMult;
			}
			++line;
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

	private static float getTextWidth(string text, float sizeMult) {
		return (text.Length * letterWidth + (text.Length - 1) * letterGap) * sizeMult;
	}

	private static float getTextHeight(float sizeMult) {
		return (letterHeight + lineGap) *sizeMult;
	}

	private FireflyFont() {}

	private const int letterWidth = 5;
	private const int letterHeight = 9; // 7 for primary, 2 for hanging letters
	private const float letterGap = 1;
	private const float lineGap = 1;

	private static readonly Dictionary<char, Vector2[]> characters = new Dictionary<char, Vector2[]> {



/////////////////////////////    NUMBERS    /////////////////////////////
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



/////////////////////////////    SYMBOLS    /////////////////////////////
		{' ', new Vector2[] {	
			}},
		{'_', new Vector2[] {
				new Vector2(0, 6),
				new Vector2(1, 6),
				new Vector2(2, 6),
				new Vector2(3, 6),
				new Vector2(4, 6),
			}},
		{'-', new Vector2[] {
				new Vector2(0, 3),
				new Vector2(1, 3),
				new Vector2(2, 3),
				new Vector2(3, 3),
				new Vector2(4, 3),
			}},
		{'+', new Vector2[] {
				new Vector2(0, 3),
				new Vector2(1, 3),
				new Vector2(2, 3),
				new Vector2(3, 3),
				new Vector2(4, 3),
				new Vector2(2, 1),
				new Vector2(2, 2),
				new Vector2(2, 4),
				new Vector2(2, 5),
			}},
		{'.', new Vector2[] {
				new Vector2(1, 6),
			}},
		{':', new Vector2[] {
				new Vector2(1, 6),
				new Vector2(1, 2),
			}},
		{'|', new Vector2[] {
				new Vector2(2, 0),
				new Vector2(2, 1),
				new Vector2(2, 2),
				new Vector2(2, 3),
				new Vector2(2, 4),
				new Vector2(2, 5),
				new Vector2(2, 6),
				new Vector2(2, 7),
			}},



/////////////////////////////    LETTERS    /////////////////////////////
		{'a', new Vector2[] {
				new Vector2(0, 2),
				new Vector2(1, 2),
				new Vector2(2, 2),
				new Vector2(3, 2),
				new Vector2(4, 3),
				new Vector2(4, 4),
				new Vector2(4, 5),
				new Vector2(4, 6),
				new Vector2(3, 6),
				new Vector2(2, 6),
				new Vector2(1, 6),
				new Vector2(0, 5),
				new Vector2(1, 4),
				new Vector2(2, 4),
				new Vector2(3, 4),
			}},
		{'b', new Vector2[] {
				new Vector2(0, 0),
				new Vector2(0, 1),
				new Vector2(0, 2),
				new Vector2(0, 3),
				new Vector2(0, 4),
				new Vector2(0, 5),
				new Vector2(0, 6),
				new Vector2(1, 6),
				new Vector2(2, 6),
				new Vector2(3, 6),
				new Vector2(4, 5),
				new Vector2(4, 4),
				new Vector2(4, 3),
				new Vector2(3, 2),
				new Vector2(2, 2),
				new Vector2(1, 3),
			}},
		{'c', new Vector2[] {
				new Vector2(4, 3),
				new Vector2(3, 2),
				new Vector2(2, 2),
				new Vector2(1, 2),
				new Vector2(0, 3),
				new Vector2(0, 4),
				new Vector2(0, 5),
				new Vector2(1, 6),
				new Vector2(2, 6),
				new Vector2(3, 6),
				new Vector2(4, 5),
			}},
		{'d', new Vector2[] {
				new Vector2(4, 0),
				new Vector2(4, 1),
				new Vector2(4, 2),
				new Vector2(4, 3),
				new Vector2(4, 4),
				new Vector2(4, 5),
				new Vector2(4, 6),
				new Vector2(3, 6),
				new Vector2(2, 6),
				new Vector2(1, 6),
				new Vector2(0, 5),
				new Vector2(0, 4),
				new Vector2(0, 3),
				new Vector2(1, 2),
				new Vector2(2, 2),
				new Vector2(3, 2),
			}},
		{'e', new Vector2[] {
				new Vector2(1, 4),
				new Vector2(2, 4),
				new Vector2(3, 4),
				new Vector2(4, 3),
				new Vector2(4, 2),
				new Vector2(3, 2),
				new Vector2(2, 2),
				new Vector2(1, 2),
				new Vector2(0, 3),
				new Vector2(0, 4),
				new Vector2(0, 5),
				new Vector2(1, 6),
				new Vector2(2, 6),
				new Vector2(3, 6),
				new Vector2(4, 6),
			}},
		{'f', new Vector2[] {
				new Vector2(4, 0),
				new Vector2(3, 0),
				new Vector2(2, 0),
				new Vector2(1, 0),
				new Vector2(1, 1),
				new Vector2(1, 2),
				new Vector2(1, 3),
				new Vector2(1, 4),
				new Vector2(1, 5),
				new Vector2(1, 6),
				new Vector2(0, 2),
				new Vector2(2, 2),
				new Vector2(3, 2),
				new Vector2(4, 2),
			}},
		{'g', new Vector2[] {
				new Vector2(3, 6),
				new Vector2(2, 6),
				new Vector2(1, 6),
				new Vector2(0, 5),
				new Vector2(0, 4),
				new Vector2(0, 3),
				new Vector2(1, 2),
				new Vector2(2, 2),
				new Vector2(3, 2),
				new Vector2(4, 2),
				new Vector2(4, 3),
				new Vector2(4, 4),
				new Vector2(4, 5),
				new Vector2(4, 6),
				new Vector2(4, 7),
				new Vector2(3, 8),
				new Vector2(2, 8),
				new Vector2(1, 8),
				new Vector2(0, 8),
			}},
		{'h', new Vector2[] {
				new Vector2(0, 0),
				new Vector2(0, 1),
				new Vector2(0, 2),
				new Vector2(0, 3),
				new Vector2(0, 4),
				new Vector2(0, 5),
				new Vector2(0, 6),
				new Vector2(1, 3),
				new Vector2(2, 2),
				new Vector2(3, 2),
				new Vector2(4, 3),
				new Vector2(4, 4),
				new Vector2(4, 5),
				new Vector2(4, 6),
			}},
		{'i', new Vector2[] {
				new Vector2(2, 1),
				new Vector2(2, 3),
				new Vector2(2, 4),
				new Vector2(2, 5),
				new Vector2(2, 6),
			}},
		{'j', new Vector2[] {
				new Vector2(3, 1),
				new Vector2(3, 3),
				new Vector2(3, 4),
				new Vector2(3, 5),
				new Vector2(3, 6),
				new Vector2(3, 7),
				new Vector2(2, 8),
				new Vector2(1, 8),
			}},
		{'k', new Vector2[] {
				new Vector2(0, 0),
				new Vector2(0, 1),
				new Vector2(0, 2),
				new Vector2(0, 3),
				new Vector2(0, 4),
				new Vector2(0, 5),
				new Vector2(0, 6),
				new Vector2(1, 4),
				new Vector2(2, 3),
				new Vector2(3, 2),
				new Vector2(2, 5),
				new Vector2(3, 6),
			}},
		{'l', new Vector2[] {
				new Vector2(2, 0),
				new Vector2(2, 1),
				new Vector2(2, 2),
				new Vector2(2, 3),
				new Vector2(2, 4),
				new Vector2(2, 5),
				new Vector2(2, 6),
			}},
		{'m', new Vector2[] {
				new Vector2(0, 6),
				new Vector2(0, 5),
				new Vector2(0, 4),
				new Vector2(0, 3),
				new Vector2(0, 2),
				new Vector2(1, 2),
				new Vector2(2, 3),
				new Vector2(2, 4),
				new Vector2(2, 5),
				new Vector2(2, 6),
				new Vector2(3, 2),
				new Vector2(4, 3),
				new Vector2(4, 4),
				new Vector2(4, 5),
				new Vector2(4, 6),
			}},
		{'n', new Vector2[] {
				new Vector2(0, 6),
				new Vector2(0, 5),
				new Vector2(0, 4),
				new Vector2(0, 3),
				new Vector2(0, 2),
				new Vector2(1, 2),
				new Vector2(2, 2),
				new Vector2(3, 2),
				new Vector2(4, 3),
				new Vector2(4, 4),
				new Vector2(4, 5),
				new Vector2(4, 6),
			}},
		{'o', new Vector2[] {
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
				new Vector2(0, 4),
				new Vector2(0, 3),
			}},
		{'p', new Vector2[] {
				new Vector2(0, 8),
				new Vector2(0, 7),
				new Vector2(0, 6),
				new Vector2(0, 5),
				new Vector2(0, 4),
				new Vector2(0, 3),
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
			}},
		{'q', new Vector2[] {
				new Vector2(4, 8),
				new Vector2(4, 7),
				new Vector2(4, 6),
				new Vector2(4, 5),
				new Vector2(4, 4),
				new Vector2(4, 3),
				new Vector2(4, 2),
				new Vector2(3, 2),
				new Vector2(2, 2),
				new Vector2(1, 2),
				new Vector2(0, 3),
				new Vector2(0, 4),
				new Vector2(0, 5),
				new Vector2(1, 6),
				new Vector2(2, 6),
				new Vector2(3, 6),
			}},
		{'r', new Vector2[] {
				new Vector2(0, 6),
				new Vector2(0, 5),
				new Vector2(0, 4),
				new Vector2(0, 3),
				new Vector2(0, 2),
				new Vector2(1, 2),
				new Vector2(2, 2),
				new Vector2(3, 2),
				new Vector2(4, 3),
			}},
		{'s', new Vector2[] {
				new Vector2(4, 2),
				new Vector2(3, 2),
				new Vector2(2, 2),
				new Vector2(1, 2),
				new Vector2(0, 3),
				new Vector2(1, 4),
				new Vector2(2, 4),
				new Vector2(3, 4),
				new Vector2(4, 5),
				new Vector2(3, 6),
				new Vector2(2, 6),
				new Vector2(1, 6),
				new Vector2(0, 6),
			}},
		{'t', new Vector2[] {
				new Vector2(1, 1),
				new Vector2(1, 2),
				new Vector2(1, 3),
				new Vector2(1, 4),
				new Vector2(1, 5),
				new Vector2(2, 6),
				new Vector2(3, 6),
				new Vector2(0, 2),
				new Vector2(2, 2),
				new Vector2(3, 2),
			}},
		{'u', new Vector2[] {
				new Vector2(0, 2),
				new Vector2(0, 3),
				new Vector2(0, 4),
				new Vector2(0, 5),
				new Vector2(1, 6),
				new Vector2(2, 6),
				new Vector2(3, 6),
				new Vector2(4, 6),
				new Vector2(4, 5),
				new Vector2(4, 4),
				new Vector2(4, 3),
				new Vector2(4, 2),
			}},
		{'v', new Vector2[] {
				new Vector2(0, 2),
				new Vector2(0, 3),
				new Vector2(1, 4),
				new Vector2(1, 5),
				new Vector2(2, 6),
				new Vector2(3, 5),
				new Vector2(3, 4),
				new Vector2(4, 3),
				new Vector2(4, 2),
			}},
		{'w', new Vector2[] {
				new Vector2(0, 2),
				new Vector2(0, 3),
				new Vector2(0, 4),
				new Vector2(0, 5),
				new Vector2(1, 6),
				new Vector2(2, 5),
				new Vector2(2, 4),
				new Vector2(2, 3),
				new Vector2(2, 2),
				new Vector2(3, 6),
				new Vector2(4, 5),
				new Vector2(4, 4),
				new Vector2(4, 3),
				new Vector2(4, 2),
			}},
		{'x', new Vector2[] {
				new Vector2(0, 2),
				new Vector2(1, 3),
				new Vector2(2, 4),
				new Vector2(3, 5),
				new Vector2(4, 6),
				new Vector2(0, 6),
				new Vector2(1, 5),
				new Vector2(3, 3),
				new Vector2(4, 2),
			}},
		{'y', new Vector2[] {
				new Vector2(0, 2),
				new Vector2(0, 3),
				new Vector2(0, 4),
				new Vector2(0, 5),
				new Vector2(1, 6),
				new Vector2(2, 6),
				new Vector2(3, 6),
				new Vector2(4, 2),
				new Vector2(4, 3),
				new Vector2(4, 4),
				new Vector2(4, 5),
				new Vector2(4, 6),
				new Vector2(4, 7),
				new Vector2(3, 8),
				new Vector2(2, 8),
				new Vector2(1, 8),
				new Vector2(0, 8),
			}},
		{'z', new Vector2[] {
				new Vector2(0, 2),
				new Vector2(1, 2),
				new Vector2(2, 2),
				new Vector2(3, 2),
				new Vector2(4, 2),
				new Vector2(3, 3),
				new Vector2(2, 4),
				new Vector2(1, 5),
				new Vector2(0, 6),
				new Vector2(1, 6),
				new Vector2(2, 6),
				new Vector2(3, 6),
				new Vector2(4, 6),
			}},
	};

}
