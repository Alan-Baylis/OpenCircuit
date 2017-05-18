using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using NUnit.Framework;

public class ScoreTest {

	private Score scoreComponent;

	private bool recieved;
	private bool recievedTeam;
	private float value;
	private int team;

	// A UnityTest behaves like a coroutine in PlayMode
	// and allows you to yield null to skip a frame in EditMode
	[UnityTest]
	public IEnumerator testScoreOnDestroy() {
		EventManager.registerForEvent(typeof(ScoreEvent), score);

		GameObject clientControllerObject = new GameObject();
		ClientController clientController = clientControllerObject.AddComponent<ClientController>();
		clientController.enabled = false;

		scoreComponent = createScore();
		scoreComponent.owner = clientController;
		scoreComponent.value = 100f;
		yield return null;
		GameObject.Destroy(scoreComponent.gameObject);
		yield return null;
		LogAssert.NoUnexpectedReceived();
		Assert.That(recieved);
		Assert.That(value, Is.EqualTo(-100f).Within(.00001f));

		cleanup();
	}

	[UnityTest]
	public IEnumerator testScoreOnDestroy_TeamOwned() {
		EventManager.registerForEvent(typeof(ScoreEvent), score);
		EventManager.registerForEvent(typeof(TeamScoreEvent), teamScore);

		scoreComponent = createScore();
		scoreComponent.value = 100f;
		TeamId teamId = scoreComponent.gameObject.AddComponent<TeamId>();
		scoreComponent.teamOwned = true;
		yield return null;
		GameObject.Destroy(scoreComponent.gameObject);
		yield return null;
		LogAssert.NoUnexpectedReceived();
		Assert.That(recievedTeam);
		Assert.That(value, Is.EqualTo(-100).Within(.00001f));
		Assert.That(team, Is.EqualTo(teamId.id));

		cleanup();
	}

	[UnityTest]
	public IEnumerator testOnScore() {
		EventManager.registerForEvent(typeof(ScoreEvent), score);

		scoreComponent = createScore();
		scoreComponent.value = 100;
		TeamId team = scoreComponent.gameObject.AddComponent<TeamId>();
		team.id = 1;
		yield return null;
		scoreComponent.recordScore(null);
		yield return null;
		LogAssert.NoUnexpectedReceived();
		Assert.That(recieved);
		Assert.That(value, Is.EqualTo(100f).Within(.00001f));

		cleanup();
	}

	[UnityTest]
	public IEnumerator testOnScore_SameTeam() {
		EventManager.registerForEvent(typeof(ScoreEvent), score);

		scoreComponent = createScore();
		scoreComponent.value = 100;
		TeamId team = scoreComponent.gameObject.AddComponent<TeamId>();
		team.id = 0;
		yield return null;
		scoreComponent.recordScore(null);
		yield return null;
		LogAssert.NoUnexpectedReceived();
		Assert.That(recieved);
		Assert.That(value, Is.EqualTo(-100f).Within(.00001f));

		cleanup();
	}

	private void cleanup() {
		EventManager.unregisterForEvent(typeof(ScoreEvent), score);
		recieved = false;
		recievedTeam = false;
		if (scoreComponent != null)
			GameObject.Destroy(scoreComponent.gameObject);
	}

	private void score(AbstractEvent incomingEvent) {
		recieved = true;
		value = ((ScoreEvent) incomingEvent).getScore();
	}

	private void teamScore(AbstractEvent incomingEvent) {
		TeamScoreEvent teamEvent = (TeamScoreEvent) incomingEvent;
		recievedTeam = true;
		value = teamEvent.getScore();
		team = teamEvent.getTeam();
	}

	private Score createScore() {
		GameObject gameObject = new GameObject("");
		gameObject.AddComponent<Label>();
		return gameObject.AddComponent<Score>();
	}
}
