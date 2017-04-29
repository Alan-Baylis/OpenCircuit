using System;
using System.Collections.Generic;
using UnityEngine;

public class Scoreboard : MonoBehaviour {

	public float updateInterval = 1f;

	private const string HEADER_KEY = "scoreboard";
	private const string ENTRY_KEY_PREFIX = "score-";
	private int scoreEntries;
	private float lastUpdate;

	private Bases myBases;

	private Bases bases {
		get {
			if (myBases == null) {
				myBases = GlobalConfig.globalConfig.gamemode as Bases;

			}
			return myBases;
		}
	}

	// Update is called once per frame
	void Update () {
		if (GlobalConfig.globalConfig != null && bases != null) {
			if (Time.time - lastUpdate > updateInterval) {
				lastUpdate = Time.time;
				if (scoreEntries != bases.clientInfoMap.Count) {
					clearScoreboardDisplay();
					scoreEntries = bases.clientInfoMap.Count;
				}

				Color prevColor = HUD.hud.fireflyConfig.fireflyColor;
				HUD.hud.fireflyConfig.fireflyColor = new Color(.25f, .25f, 1);
				HUD.hud.setFireflyElement(HEADER_KEY, this,
					FireflyFont.getString("scoreboard", .2f, new Vector2(0f, -.45f), FireflyFont.HAlign.CENTER), false);

				PriorityQueue priorityQueue = new PriorityQueue();
				foreach (KeyValuePair<ClientController, Bases.ClientInfo> info in bases.clientInfoMap) {
					priorityQueue.Enqueue(new FloatPrioritizable(info.Key.playerName,
						Bases.adjustScoreForTime(info.Value.score.total, info.Key.startTime)));

				}
				int index = 0;
				while (priorityQueue.peek() != null) {
					FloatPrioritizable entry = priorityQueue.Dequeue() as FloatPrioritizable;
					string entryString = entry.getName().Substring(0, Math.Min(entry.getName().Length, 10)).PadRight(10);
					entryString += entry.getPriority().ToString("0.").PadLeft(15);
					HUD.hud.setFireflyElement(ENTRY_KEY_PREFIX + index, this,
						FireflyFont.getString(entryString, .1f, new Vector2(0f, -.3f + index * 0.1f), FireflyFont.HAlign.CENTER), false);
					++index;
				}

				HUD.hud.fireflyConfig.fireflyColor = prevColor;
			}
		} else {
			clearScoreboardDisplay();
		}
	}

	private void clearScoreboardDisplay() {
		HUD.hud.clearFireflyElement(HEADER_KEY);
		for (int i = 0; i < scoreEntries; i++) {
			HUD.hud.clearFireflyElement(ENTRY_KEY_PREFIX + i);
		}
	}

	void OnDisable() {
		clearScoreboardDisplay();
	}

	private class FloatPrioritizable : Prioritizable{
		private float value;
		private string name;

		public FloatPrioritizable(string name, float value) {
			this.value = value;
			this.name = name;
		}

		public string getName() {
			return name;
		}

		public float getPriority() {
			return value;
		}
	}
}
