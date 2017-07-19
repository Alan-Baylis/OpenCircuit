#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.Networking;

public class GlobalConfigWinLoseTest {

	private bool won;
	private bool lost;

	// A UnityTest behaves like a coroutine in PlayMode
	// and allows you to yield null to skip a frame in EditMode
	[UnityTest]
	[PrebuildSetup(typeof(NetworkSetup))]
	public IEnumerator testWinLose() {
		GameObject globalConfigObject = new GameObject();
		try {
			EventManager.getGameControlChannel().registerForEvent(typeof(WinEvent), win);
			EventManager.getGameControlChannel().registerForEvent(typeof(LoseEvent), lose);
			GlobalConfig globalConfig = globalConfigObject.AddComponent<GlobalConfig>();
			globalConfig.gamemode = globalConfigObject.AddComponent<Bases>();
			globalConfig.cameraManager = globalConfigObject.AddComponent<CameraManager>();
			NetworkServer.Spawn(globalConfigObject);
			yield return null;

			Assert.That(!won);
			Assert.That(!lost);

//			globalConfig.winGame();
			yield return null;
			Assert.That(won);

			//globalConfig.loseGame();
			yield return null;
			Assert.That(lost);
		} finally {
			GameObject.Destroy(globalConfigObject);
		}
		yield return null;
		LogAssert.NoUnexpectedReceived();

	}

	private void win(AbstractEvent winEvent) {
		won = true;
	}

	private void lose(AbstractEvent loseEvent) {
		lost = true;
	}
}
#endif
