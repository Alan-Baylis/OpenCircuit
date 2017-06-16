#if UNITY_EDITOR
using UnityEngine.TestTools;
using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;

public class AttachRobotComponentTest {

	private RobotController robotController;

	[PrebuildSetup(typeof(NetworkSetup))]
	[UnityTest]
	public IEnumerator testAttach_RoboEyesNoPowerSource() {
		PlayModeTestUtility.setupNavMesh();
		Regex noPowerSource = new Regex("has no power source!");
		LogAssert.Expect(LogType.Log, noPowerSource);
		robotController = PlayModeTestUtility.createRobot();
		RoboEyes component = PlayModeTestUtility.addRobotComponent<RoboEyes>(robotController);
		NetworkServer.Spawn(component.gameObject);
		NetworkServer.Spawn(robotController.gameObject);

		yield return new WaitForSeconds(component.lookAroundInterval*2);
		yield return null;
		LogAssert.NoUnexpectedReceived();
		Assert.That(robotController.getRobotComponent<RoboEyes>(), Is.Not.Null);

		cleanup();
	}

	[PrebuildSetup(typeof(NetworkSetup))]
	[UnityTest]
	public IEnumerator testAttach_RoboEyes() {
		PlayModeTestUtility.setupNavMesh();
		robotController = PlayModeTestUtility.createRobot();
		var component = PlayModeTestUtility.addRobotComponent<RoboEyes>(robotController).gameObject;
		NetworkServer.Spawn(component);
		NetworkServer.Spawn(robotController.gameObject);

		yield return null;
		LogAssert.NoUnexpectedReceived();
		Assert.That(robotController.getRobotComponent<RoboEyes>(), Is.Not.Null);

		cleanup();
	}

	[PrebuildSetup(typeof(NetworkSetup))]
	[UnityTest]
	public IEnumerator testAttach_HoverJet() {
		PlayModeTestUtility.setupNavMesh();
		robotController = PlayModeTestUtility.createRobot();
		PlayModeTestUtility.addRobotComponent<PowerGenerator>(robotController);
		var component = PlayModeTestUtility.addRobotComponent<HoverJet>(robotController).gameObject;
		NetworkServer.Spawn(component);
		NetworkServer.Spawn(robotController.gameObject);

		yield return null;
		LogAssert.NoUnexpectedReceived();
		Assert.That(robotController.getRobotComponent<HoverJet>(), Is.Not.Null);

		cleanup();
	}

	[PrebuildSetup(typeof(NetworkSetup))]
	[UnityTest]
	public IEnumerator testAttach_AudioSensor() {
		PlayModeTestUtility.setupNavMesh();
		robotController = PlayModeTestUtility.createRobot();
		PlayModeTestUtility.addRobotComponent<PowerGenerator>(robotController);
		var component = PlayModeTestUtility.addRobotComponent<AudioSensor>(robotController).gameObject;
		NetworkServer.Spawn(component);
		NetworkServer.Spawn(robotController.gameObject);

		yield return null;
		LogAssert.NoUnexpectedReceived();
		Assert.That(robotController.getRobotComponent<AudioSensor>(), Is.Not.Null);

		cleanup();
	}

	private void cleanup() {
		GameObject.Destroy(robotController.gameObject);
	}
}
#endif
