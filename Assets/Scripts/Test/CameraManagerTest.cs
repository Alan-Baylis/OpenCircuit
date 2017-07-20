#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.Networking;

public class CameraManagerTest {
	private GlobalConfig globalConfig;
	private ClientController clientController1;
	private ClientController clientController2;
	private Camera cam1;
	private Camera cam2;
	private Player player1;
	private Player player2;
	private CameraManager cameraManager;

	[PrebuildSetup(typeof(NetworkSetup))]
	[UnityTest]
	public IEnumerator testUsePlayerCam() {
		setupPart1();
		yield return null;
		setupPart2();
		yield return null;
		try {
			Assert.That(cameraManager.getSceneCamera().enabled);

			yield return null;
			cameraManager.usePlayerCam(cam1);
			Assert.That(cam1.enabled);
			Assert.That(!cam2.enabled);
			Assert.That(!cameraManager.getSceneCamera().enabled);
		} finally {
			cleanup();
		}

		yield return null;
		LogAssert.NoUnexpectedReceived();
	}

	[PrebuildSetup(typeof(NetworkSetup))]
	[UnityTest]
	public IEnumerator testSwitchCameras() {
		setupPart1();
		yield return null;
		setupPart2();
		yield return null;
		try {
			Assert.That(cameraManager.getSceneCamera().enabled);

			yield return null;

			cameraManager.switchCamera();
			Assert.That(cameraManager.getSceneCamera().enabled);
			Assert.That(!cam1.enabled);
			Assert.That(!cam2.enabled);

			yield return null;

			cameraManager.switchCamera();
			Assert.That(!cameraManager.getSceneCamera().enabled);
			Assert.That(cam1.enabled);
			Assert.That(!cam2.enabled);
			yield return null;

			cameraManager.switchCamera();
			Assert.That(!cameraManager.getSceneCamera().enabled);
			Assert.That(!cam1.enabled);
			Assert.That(cam2.enabled);
			yield return null;
		} finally {
			cleanup();
		}
		yield return null;
		LogAssert.NoUnexpectedReceived();
	}

	[PrebuildSetup(typeof(NetworkSetup))]
	[UnityTest]
	public IEnumerator testRemoveCamera() {
		setupPart1();
		yield return null;
		setupPart2();
		try {
			yield return null;
			cameraManager.switchCamera();
			yield return null;
			cameraManager.switchCamera();
			Assert.That(!cameraManager.getSceneCamera().enabled);
			Assert.That(cam1.enabled);
			Assert.That(!cam2.enabled);
			yield return null;
			GameObject.Destroy(player1.gameObject);
			yield return null;
			Assert.That(!cam2.enabled);
			Assert.That(cameraManager.getSceneCamera().enabled);
		} finally {
			cleanup();
		}
		yield return null;
		LogAssert.NoUnexpectedReceived();
	}

	private void setupPart1() {
		GameObject globalConfigObject = new GameObject("GlobalConfig");
		globalConfig = globalConfigObject.AddComponent<GlobalConfig>();
		globalConfigObject.AddComponent<Bases>();
		globalConfig.configuration.gameMode = GameMode.GameModes.BASES;
		cameraManager = globalConfigObject.AddComponent<CameraManager>();
		globalConfig.cameraManager = cameraManager;
	}

	private void setupPart2() {

	clientController1 = PlayModeTestUtility.createClientController();
		clientController2 = PlayModeTestUtility.createClientController();
		clientController1.enabled = false;
		clientController2.enabled = false;

		NetworkServer.Spawn(clientController1.gameObject);
		NetworkServer.Spawn(clientController2.gameObject);

		player1 = PlayModeTestUtility.createPlayer();
		player2 = PlayModeTestUtility.createPlayer();

		player1.clientControllerId = clientController1.netId;
		player2.clientControllerId = clientController2.netId;

		cam1 = createCameraForPlayer(player1);
		cam2 = createCameraForPlayer(player2);
	}

	private void cleanup() {
		cameraManager.useSceneCamera();

		if (player1!=null)
			GameObject.Destroy(player1.gameObject);
		if (player2 != null)
			GameObject.Destroy(player2.gameObject);
		GameObject.Destroy(clientController1.gameObject);
		GameObject.Destroy(clientController2.gameObject);
		GameObject.Destroy(globalConfig.gameObject);
	}

	private Camera createCameraForPlayer(Player player) {
		GameObject cameraObject = new GameObject("playerCam");
		cameraObject.transform.parent = player.transform;
		cameraObject.AddComponent<PlayerCamera>();
		cameraObject.AddComponent<AudioListener>().enabled = false;
		Camera cam = cameraObject.AddComponent<Camera>();
		cam.enabled = false;
		return cam;
	}
}
#endif
