using UnityEngine;
using NUnit.Framework;

public class RobotControllerTest {

	[Test]
	public void testAddSighting() {
		//Arrange
		var gameObject = new GameObject();
		RobotController robotController = gameObject.AddComponent<RobotController>();

		LabelHandle myLabelHandle = new LabelHandle(Vector3.zero, "testLH");
		myLabelHandle.addTag(new Tag(TagEnum.Grabbed, 0, myLabelHandle));
		robotController.sightingFound(myLabelHandle, Vector3.zero, null);

		Debug.Log(robotController.getMentalModel());
		Assert.True(robotController.knowsTarget(myLabelHandle));
		robotController.sightingLost(myLabelHandle, Vector3.back, null);
		Assert.False(robotController.knowsTarget(myLabelHandle));
	}

	[Test]
	public void testAddSighting_WithoutTags() {
		//Arrange
		var gameObject = new GameObject();
		RobotController robotController = gameObject.AddComponent<RobotController>();

		LabelHandle myLabelHandle = new LabelHandle(Vector3.zero, "testLH");
		robotController.sightingFound(myLabelHandle, Vector3.zero, null);

		Debug.Log(robotController.getMentalModel());
		Assert.False(robotController.knowsTarget(myLabelHandle));

	}
}
