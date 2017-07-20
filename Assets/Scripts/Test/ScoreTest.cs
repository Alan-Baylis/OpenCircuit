#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using NUnit.Framework;
using UnityEngine.Networking;

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
		GameObject clientControllerObject = new GameObject();
		GlobalConfig globalConfig = PlayModeTestUtility.createGlobalConfig<Bases>();
		try {
			globalConfig.gamemode.enabled = false;
			NetworkServer.Spawn(globalConfig.gameObject);
			EventManager.getInGameChannel().registerForEvent(typeof(ScoreEvent), score);

			ClientController clientController = clientControllerObject.AddComponent<ClientController>();
			clientController.enabled = false;
			globalConfig.clients.Add(clientController);
			globalConfig.localClient = clientController;
			NetworkServer.Spawn(clientController.gameObject);

			scoreComponent = PlayModeTestUtility.createScore();
			scoreComponent.owner = clientController;
			scoreComponent.value = 100f;
			yield return null;
			GameObject.Destroy(scoreComponent.gameObject);
			yield return null;
			Assert.That(recieved);
			Assert.That(value, Is.EqualTo(-100f).Within(.00001f));
		} finally {

			GameObject.Destroy(clientControllerObject);
			GameObject.Destroy(globalConfig.gameObject);
			cleanup();
		}
		yield return null;
		LogAssert.NoUnexpectedReceived();
	}

	[UnityTest]
	public IEnumerator testScoreOnDestroy_TeamOwned() {
		try {
			EventManager.getInGameChannel().registerForEvent(typeof(ScoreEvent), score);
			EventManager.getInGameChannel().registerForEvent(typeof(TeamScoreEvent), teamScore);

			scoreComponent = PlayModeTestUtility.createScore();
			scoreComponent.value = 100f;
			TeamId teamId = scoreComponent.gameObject.AddComponent<TeamId>();
			scoreComponent.teamOwned = true;
			yield return null;
			GameObject.Destroy(scoreComponent.gameObject);
			yield return null;
			Assert.That(recievedTeam);
			Assert.That(value, Is.EqualTo(-100).Within(.00001f));
			Assert.That(team, Is.EqualTo(teamId.id));

		} finally {
			cleanup();
		}
		yield return null;
		LogAssert.NoUnexpectedReceived();
	}

	[UnityTest]
	public IEnumerator testOnScore() {
		try {
			EventManager.getInGameChannel().registerForEvent(typeof(ScoreEvent), score);

			scoreComponent = PlayModeTestUtility.createScore();
			scoreComponent.value = 100;
			TeamId team = scoreComponent.gameObject.AddComponent<TeamId>();
			team.id = 1;
			yield return null;
			scoreComponent.recordScore(null);
			yield return null;
			LogAssert.NoUnexpectedReceived();
			Assert.That(recieved);
			Assert.That(value, Is.EqualTo(100f).Within(.00001f));
		} finally {
			cleanup();
		}
		yield return null;
		LogAssert.NoUnexpectedReceived();
	}

	[UnityTest]
	public IEnumerator testOnScore_SameTeam() {
		try {
			EventManager.getInGameChannel().registerForEvent(typeof(ScoreEvent), score);

			scoreComponent = PlayModeTestUtility.createScore();
			scoreComponent.value = 100;
			TeamId team = scoreComponent.gameObject.AddComponent<TeamId>();
			team.id = 0;
			yield return null;
			scoreComponent.recordScore(null);
			yield return null;
			LogAssert.NoUnexpectedReceived();
			Assert.That(recieved);
			Assert.That(value, Is.EqualTo(-100f).Within(.00001f));
		} finally {
			cleanup();
		}
		yield return null;
		LogAssert.NoUnexpectedReceived();
	}

	private void cleanup() {
		EventManager.getInGameChannel().unregisterForEvent(typeof(ScoreEvent), score);
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
}
#endif
