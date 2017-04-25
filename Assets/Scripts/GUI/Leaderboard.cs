using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Leaderboard : NetworkBehaviour {

	public class LeaderboardSyncClass : SyncListStruct<LeaderboardEntry> {

	}

	public LeaderboardSyncClass leaderboardEntries = new LeaderboardSyncClass();


	public int entries;

	private List<LeaderboardEntry> scores;

	[Server]
	void Start() {
		scores = new List<LeaderboardEntry>();
		for (int i = 0; i < entries; ++i) {
			if (PlayerPrefs.HasKey("scoreName" + i)) {
				LeaderboardEntry entry = new LeaderboardEntry(PlayerPrefs.GetString("scoreName" + i),
					PlayerPrefs.GetFloat("score" + i));
				scores.Add(entry);
				leaderboardEntries.Add(entry);
			} else {
				break;
			}
		}
	}

	[ClientCallback]
	void Update() {
		if (GlobalConfig.globalConfig.localClient != null && GlobalConfig.globalConfig.localClient.spectator && GlobalConfig.globalConfig.cameraManager.getSceneCamera()
			    .enabled) {

			Color prevColor = HUD.hud.fireflyConfig.fireflyColor;
			HUD.hud.fireflyConfig.fireflyColor = new Color(.25f, .25f, 1);
			HUD.hud.setFireflyElement("leaderboard", this,
				FireflyFont.getString("leaderboard", .2f, new Vector2(0f, -.45f), FireflyFont.HAlign.CENTER), false);
			for (int i = 0; i < leaderboardEntries.Count; i++) {
				LeaderboardEntry entry = leaderboardEntries[i];
				string entryString = entry.name.Substring(0, Math.Min(entry.name.Length, 10)).PadRight(10);
				entryString += entry.score.ToString("0.").PadLeft(15);
				HUD.hud.setFireflyElement("leaderboard-" + i, this,
					FireflyFont.getString(entryString , .1f, new Vector2(0f, -.3f + i * 0.1f), FireflyFont.HAlign.CENTER), false);
			}
			HUD.hud.fireflyConfig.fireflyColor = prevColor;
		} else {
			clearLeaderboardDisplay();
		}
	}

	[Server]
	public void addScore(LeaderboardEntry entry) {
		if (scores.Count > 0) {

			for (int i = 0; i < scores.Count; ++i) {
				LeaderboardEntry currentEntry = scores[i];
				if (entry.score > currentEntry.score) {
					scores.Insert(i, entry);
					leaderboardEntries.Insert(i, entry);
					break;
				} else if (i == scores.Count - 1) {
					scores.Add(entry);
					leaderboardEntries.Add(entry);
					break;
				}
			}
		} else {
			scores.Add(entry);
			leaderboardEntries.Add(entry);
		}
		writeScores();
	}

	[Server]
	public void writeScores() {
		print("write scores " + scores.Count);

		for (int i = 0; i < entries && i < scores.Count; ++i) {
			LeaderboardEntry entry = scores[i];
			PlayerPrefs.SetString("scoreName" + i, entry.name);
			PlayerPrefs.SetFloat("score" + i, entry.score);
		}
		PlayerPrefs.Save();
	}

	private void clearLeaderboardDisplay() {
		HUD.hud.clearFireflyElement("leaderboard");
		for (int i = 0; i < leaderboardEntries.Count; i++) {
			HUD.hud.clearFireflyElement("leaderboard-" + i);
		}
	}

	public struct LeaderboardEntry {
		public readonly string name;
		public readonly float score;

		public LeaderboardEntry(string name, float score) {
			this.name = name;
			this.score = score;
		}
	}

}
