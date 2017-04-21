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

		Player player = instigator.GetComponentInParent<Player>();
		Score score = parent.GetComponent<Score>();
		Debug.Log("perform score by " + instigator);
		if (score != null && player != null) {
			Debug.Log("perform record score");
			score.recordScore(player.clientController);
		}
	}
}
