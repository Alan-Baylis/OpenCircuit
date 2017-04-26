using UnityEngine;

[System.Serializable]
public class CountScoreOperation : Operation {

	private static System.Type[] triggers = {
		typeof(DestructTrigger)
	};

	public override System.Type[] getTriggers() {
		return triggers;
	}

	public override void perform(GameObject instigator, Trigger trig) {
		ScoreAgent agent = instigator.GetComponentInParent<ScoreAgent>();
		Score score = parent.GetComponent<Score>();
		if (score != null && agent != null && agent.owner != null) {
			score.recordScore(agent.owner);
		}
	}
}
