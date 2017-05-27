using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.Networking;

public class RobotDestructionTest {

	private RobotController robotController;

	[PrebuildSetup(typeof(NetworkSetup))]
	[UnityTest]
	public IEnumerator testRobotDestruction() {
		robotController = PlayModeTestUtility.createRobot();
		NetworkServer.Spawn(robotController.gameObject);

		yield return null;
		robotController.dispose();
		yield return null;
		LogAssert.NoUnexpectedReceived();
		Assert.That(robotController == null);

		cleanup();
	}

	[PrebuildSetup(typeof(NetworkSetup))]
	[UnityTest]
	public IEnumerator testRobotDestruction_WithHoverJet() {
//		LogAssert.Expect(LogType.Log, "EMPTY EFFECT");
		PlayModeTestUtility.setupNavMesh();
		robotController = PlayModeTestUtility.createRobot();
		PlayModeTestUtility.addRobotComponent<PowerGenerator>(robotController);
		NetworkServer.Spawn(PlayModeTestUtility.addRobotComponent<HoverJet>(robotController).gameObject);
		NetworkServer.Spawn(robotController.gameObject);

		yield return null;
		robotController.dispose();
		yield return null;
		LogAssert.NoUnexpectedReceived();
		Assert.That(robotController == null);

		cleanup();
	}

	[PrebuildSetup(typeof(NetworkSetup))]
	[UnityTest]
	public IEnumerator testRobotDestruction_WithEyes() {
		//LogAssert.Expect(LogType.Log, "EMPTY EFFECT");
		robotController = PlayModeTestUtility.createRobot();
		PlayModeTestUtility.addRobotComponent<PowerGenerator>(robotController);
		PrefabCatalog catalog = Resources.Load<PrefabCatalog>("Test/PrefabCatalog");
		RoboEyes component = GameObject.Instantiate(catalog.roboEyesPrefab, robotController.transform);
		NetworkServer.Spawn(component.gameObject);
		NetworkServer.Spawn(robotController.gameObject);

		yield return null;
		robotController.dispose();
		yield return null;
		LogAssert.NoUnexpectedReceived();
		Assert.That(robotController == null);

		cleanup();
	}

//	[PrebuildSetup(typeof(NetworkSetup))]
//	[UnityTest]
//	public IEnumerator testRobotDestruction_WithRifle() {
//
//		LogAssert.Expect(LogType.Log, "EMPTY EFFECT");
//		robotController = PlayModeTestUtility.createRobot();
//		PlayModeTestUtility.addRobotComponent<PowerGenerator>(robotController);
//		NetworkServer.Spawn(PlayModeTestUtility.addRobotComponent<RoboRifle>(robotController).gameObject);
//		NetworkServer.Spawn(robotController.gameObject);
//
//		yield return null;
//		robotController.dispose();
//		yield return null;
//		LogAssert.NoUnexpectedReceived();
//		Assert.That(robotController == null);
//
//		cleanup();
//	}

	private void cleanup() {
	}
}
