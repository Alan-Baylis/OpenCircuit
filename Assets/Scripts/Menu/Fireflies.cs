using System.Collections.Generic;
using UnityEngine;

public class Fireflies {
	
	public Config config;

	private List<Firefly> fireflies = new List<Firefly>();
	private int targetCount;

	public void Update() {
		float deltaTime = Mathf.Min(Time.deltaTime, 0.05f);
		int i = 0;
		foreach (Firefly fly in fireflies) {
			if (i < targetCount) {
				if (fly.alpha != 1)
					fly.alpha = Mathf.Min(1, fly.alpha +config.fadeSpeed * deltaTime);
			} else {
				if (fly.alpha != 0)
					fly.alpha = Mathf.Max(0, fly.alpha -config.fadeSpeed * deltaTime);
			}
			++i;

			Vector2 diff = fly.target -fly.position;
			if (diff.sqrMagnitude < (fly.velocity *deltaTime).sqrMagnitude &&
					Vector2.Dot(diff.normalized, fly.velocity.normalized) > 0.99f) {
				fly.position = fly.target;
				fly.velocity = Vector2.zero;
				continue;
			}
			fly.position += fly.velocity *deltaTime *config.fireflySize;
			diff = fly.target -fly.position;
			if (diff.sqrMagnitude < 1 /config.fireflySize) {
				fly.position = fly.target;
				fly.velocity = Vector2.zero;
				continue;
			}
			float diffMult = Mathf.Max(1, Mathf.Min(5, Mathf.Pow(diff.magnitude, 1)));
			if (float.IsNaN(diffMult))
				diffMult = 1;
			if (fly.velocity.sqrMagnitude != 0) {
				float mult = Mathf.Min(1, Mathf.Max(Mathf.Pow(
					(1 + Vector2.Dot (diff.normalized, fly.velocity.normalized)) / 2, diffMult * config.damper * deltaTime)));
				if (float.IsNaN(mult))
					mult = 0.5f;
				fly.velocity *= mult;
			}
			fly.velocity += diff.normalized *config.fireflyGravity *deltaTime /Mathf.Min(Mathf.Max(diffMult, 1), 5);
		}

		for (int j=fireflies.Count-1; j>=targetCount; --j) {
			if (fireflies[j].alpha == 0 || fireflies[j].target == fireflies[j].position)
				fireflies.RemoveAt(j);
			else
				break;
		}
	}

	public void OnGUI(Vector2 position) {
		foreach(Firefly fly in fireflies) {
			Color col = config.fireflyColor;
			col.a = fly.alpha;
			GUI.color = col;

			float sizeMult = Mathf.Max(1, Mathf.Min(10, Mathf.Pow(fly.velocity.sqrMagnitude, 0.1f)));
			Vector2 flyPosition = position + fly.position;
			GUI.DrawTexture(centeredRect(flyPosition, Vector2.one *config.fireflySize), config.fireflyTexture);
			GUI.DrawTexture(centeredRect(flyPosition, Vector2.one *config.fireflySize *sizeMult), config.glowTexture);
		}
	}

	public void setPositions(List<Vector2> positions, bool doShuffle) {
		if (doShuffle)
			shuffle(fireflies);
		targetCount = positions.Count;
		int i = 0;
		foreach (Firefly fly in fireflies) {
			if (i < targetCount) {
				fly.target = positions[i];
				fly.velocity += new Vector2(Random.value -0.5f, Random.value -0.5f) *config.shuffleSpeed;
				++i;
			} else {
				fly.target = randomInRect(config.spawnPosition);
				fly.velocity += new Vector2(-Random.value *100 -200, Random.value *10 -5);
			}
		}
		for (; i<targetCount; ++i) {
			Firefly firefly = new Firefly();
			firefly.target = positions[i];
			firefly.position = randomInRect(config.spawnPosition);
			firefly.velocity = new Vector2(Random.value *100 +200, 0);
			fireflies.Add(firefly);
		}
	}

	private static void shuffle<T>(List<T> list) {
		for (int i = list.Count -1; i > 0; --i) {
			int j = Random.Range(0, i);
			T v = list[j];
			list[j] = list[i];
			list[i] = v;
		}
	}

	private Rect centeredRect(Vector2 position, Vector2 size) {
		return new Rect(position - size / 2, size);
	}

	private Vector2 randomInRect(Rect container) {
		return container.min + new Vector2(Random.value *container.width, Random.value *container.height);
	}

	protected class Firefly {
		public Vector2 target, position, velocity;
		public float alpha;
	}

	[System.Serializable]
	public struct Config {
		public float fireflySize;
		public float fireflyGravity;
		public float damper;
		public float randomAcceleration;
		public float shuffleSpeed;
		public float fadeSpeed;
		public Texture fireflyTexture;
		public Texture glowTexture;
		public Color fireflyColor;
		public Rect spawnPosition;
	}
}
