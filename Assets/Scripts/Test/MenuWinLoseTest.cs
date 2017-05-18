using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class MenuWinLoseTest {
	private Menu menu;

	// A UnityTest behaves like a coroutine in PlayMode
	// and allows you to yield null to skip a frame in EditMode
	[UnityTest]
	[PrebuildSetup(typeof(NetworkSetup))]
	public IEnumerator testMenuLose() {
		try {
			GameObject menuObject = new GameObject();
			menu = menuObject.AddComponent<Menu>();
			menu.background = Texture2D.whiteTexture;
			menu.skin = ScriptableObject.CreateInstance<GUISkin>();
			menu.activeAtStart = false;

			// Use the Assert class to test conditions.
			// yield to skip a frame
			yield return null;

			Assert.That(!menu.activeAtStart);

			EventManager.broadcastEvent(new LoseEvent());

			Assert.That(menu.activeAtStart);
		} finally {
			cleanup();
		}
		yield return null;
		LogAssert.NoUnexpectedReceived();
	}

	[UnityTest]
	[PrebuildSetup(typeof(NetworkSetup))]
	public IEnumerator testMenuWin() {
		try {
			GameObject menuObject = new GameObject();
			menu = menuObject.AddComponent<Menu>();
			menu.background = Texture2D.whiteTexture;
			menu.skin = ScriptableObject.CreateInstance<GUISkin>();
			menu.activeAtStart = false;

			// Use the Assert class to test conditions.
			// yield to skip a frame
			yield return null;

			Assert.That(!menu.activeAtStart);

			EventManager.broadcastEvent(new WinEvent());

			Assert.That(menu.activeAtStart);
		} finally {
			cleanup();
		}
		yield return null;
		LogAssert.NoUnexpectedReceived();
	}

	private void cleanup() {
		GameObject.Destroy(menu.gameObject);
	}
}
