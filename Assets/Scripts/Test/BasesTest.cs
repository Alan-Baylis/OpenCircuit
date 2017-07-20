#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.Networking;

public class BasesTest {

	private const float MARGIN = 2f;

	private GlobalConfig globalConfig;
	private ClientController clientController;

	[PrebuildSetup(typeof(NetworkSetup))]
	[UnityTest]
	public IEnumerator testScore() {
		try {
			GameObject globalConfigObject = new GameObject("GlobalConfig");
			globalConfigObject.AddComponent<HUD>().Start();
			globalConfig = globalConfigObject.AddComponent<GlobalConfig>();
			Bases bases = globalConfigObject.AddComponent<Bases>();
			globalConfig.configuration.gameMode = GameMode.GameModes.BASES;
			globalConfig.cameraManager = globalConfigObject.AddComponent<CameraManager>();
			NetworkServer.Spawn(globalConfigObject);
			yield return null;

			clientController = PlayModeTestUtility.createClientController();
			clientController.enabled = false;
			globalConfig.localClient = clientController;
			NetworkServer.Spawn(clientController.gameObject);
			yield return null;

			Assert.That(bases.getScore(clientController), Is.EqualTo(0).Within(.00001f));
			EventManager.broadcastEvent(new ScoreEvent(clientController, 100f), EventManager.IN_GAME_CHANNEL);
			Assert.That(bases.getScore(clientController), Is.EqualTo(100f).Within(MARGIN));
		} finally {
			cleanup();
		}
		yield return null;
		LogAssert.NoUnexpectedReceived();
	}

	[PrebuildSetup(typeof(NetworkSetup))]
	[UnityTest]
	public IEnumerator testScore_DestroyOwnAsset() {
		try {
			GameObject globalConfigObject = new GameObject("GlobalConfig");
			globalConfigObject.AddComponent<HUD>().Start();
			globalConfig = globalConfigObject.AddComponent<GlobalConfig>();
			Bases bases = globalConfigObject.AddComponent<Bases>();
			globalConfig.configuration.gameMode = GameMode.GameModes.BASES;
			globalConfig.cameraManager = globalConfigObject.AddComponent<CameraManager>();
			NetworkServer.Spawn(globalConfigObject);
			yield return null;

			clientController = PlayModeTestUtility.createClientController();
			clientController.enabled = false;
			globalConfig.localClient = clientController;
			NetworkServer.Spawn(clientController.gameObject);
			yield return null;

			Assert.That(bases.getScore(clientController), Is.EqualTo(0).Within(.00001f));
			EventManager.broadcastEvent(new ScoreEvent(clientController, -100f), EventManager.IN_GAME_CHANNEL);
			Assert.That(bases.getScore(clientController), Is.EqualTo(-100f).Within(MARGIN));

		} finally {
			cleanup();
		}
		yield return null;
		LogAssert.NoUnexpectedReceived();
	}

	[PrebuildSetup(typeof(NetworkSetup))]
	[UnityTest]
	public IEnumerator testScore_TeamOwned() {
		try {
			GameObject globalConfigObject = new GameObject("GlobalConfig");
			globalConfigObject.AddComponent<HUD>().Start();
			globalConfig = globalConfigObject.AddComponent<GlobalConfig>();
			Bases bases = globalConfigObject.AddComponent<Bases>();
			globalConfig.configuration.gameMode = GameMode.GameModes.BASES;
			globalConfig.cameraManager = globalConfigObject.AddComponent<CameraManager>();
			NetworkServer.Spawn(globalConfigObject);

			clientController = PlayModeTestUtility.createClientController();
			clientController.enabled = false;
			globalConfig.localClient = clientController;
			globalConfig.clients.Add(clientController);
			NetworkServer.Spawn(clientController.gameObject);
			yield return null;

			Assert.That(bases.getScore(clientController), Is.EqualTo(0).Within(.00001f));
			EventManager.broadcastEvent(new TeamScoreEvent(0, -100f), EventManager.IN_GAME_CHANNEL);
			Assert.That(bases.getScore(clientController), Is.EqualTo(-100f).Within(MARGIN));

		} finally {
			cleanup();
		}
		yield return null;
		LogAssert.NoUnexpectedReceived();
	}

	private void cleanup() {
		if (clientController != null)
			GameObject.Destroy(clientController);
		if (globalConfig!= null)
			GameObject.Destroy(globalConfig.gameObject);

	}
}
#endif
