using System;
using UnityEngine;
using UnityEngine.Networking;

public class Leaderboard : NetworkBehaviour {

	private const string SCORE_NAME_KEY = "scoreName";
	private const string SCORE_VALUE_KEY = "score";
	private const string SCORE_VALID_KEY = "scoreValid";

	public LeaderboardSyncClass leaderboardEntries = new LeaderboardSyncClass();

	public int entries;

	[ServerCallback]
	void Start() {
		for (int i = 0; i < entries; ++i) {
			if (PlayerPrefs.HasKey(SCORE_NAME_KEY + i) && PlayerPrefs.GetInt(SCORE_VALID_KEY+i) == 1) {
				LeaderboardEntry entry = new LeaderboardEntry(PlayerPrefs.GetString(SCORE_NAME_KEY + i),
					PlayerPrefs.GetFloat(SCORE_VALUE_KEY + i));
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
			HUD.hud.fireflyConfig.fireflyColor = new Color(.33f, .33f, 1);
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
		if (leaderboardEntries.Count > 0) {
			for (int i = 0; i < leaderboardEntries.Count; ++i) {
				LeaderboardEntry currentEntry = leaderboardEntries[i];
				if (entry.score > currentEntry.score) {
					leaderboardEntries.Insert(i, entry);
					break;
				} else if (i == leaderboardEntries.Count - 1) {
					leaderboardEntries.Add(entry);
					break;
				}
			}
		} else {
			leaderboardEntries.Add(entry);
		}
		writeScores();
	}

	[Server]
	public void removeScore(int index) {
		if (index >= 0 && index < leaderboardEntries.Count) {
			clearLeaderboardDisplay();
			leaderboardEntries.RemoveAt(index);
		}
		writeScores();
	}

	[Command]
	public void CmdRemoveScore(int index) {
		removeScore(index);
	}

	//[Either]
	public int getScoreCount() {
		return leaderboardEntries.Count;
	}

	[Server]
	public void writeScores() {
		for (int i = 0; i < entries; ++i) {
			if (i < leaderboardEntries.Count) {
				LeaderboardEntry entry = leaderboardEntries[i];
				PlayerPrefs.SetInt(SCORE_VALID_KEY+i, 1);
				PlayerPrefs.SetString(SCORE_NAME_KEY + i, entry.name);
				PlayerPrefs.SetFloat(SCORE_VALUE_KEY + i, entry.score);
			} else {
				PlayerPrefs.SetInt(SCORE_VALID_KEY+i, 0);
			}
		}
		PlayerPrefs.Save();
	}

	private void clearLeaderboardDisplay() {
		HUD.hud.clearFireflyElement("leaderboard");
		for (int i = 0; i < leaderboardEntries.Count; i++) {
			HUD.hud.clearFireflyElement("leaderboard-" + i);
		}
	}

	public class LeaderboardSyncClass : SyncListStruct<LeaderboardEntry> {

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
