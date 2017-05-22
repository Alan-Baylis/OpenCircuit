using UnityEngine;

public class Team {

	public readonly int id;
	public int robotCount;
	public Config config;

	public Team(int id, Config config) {
		this.id = id;
		this.config = config;
	}

	[System.Serializable]
	public struct Config {
		public Color color;

		public Config(Color color) {
			this.color = color;
		}
	}
}
