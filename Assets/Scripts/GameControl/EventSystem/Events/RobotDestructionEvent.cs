public class RobotDestructionEvent : AbstractEvent {
	private RobotController myRobotController;

	public RobotController robotController {
		get { return myRobotController; }
	}

	public RobotDestructionEvent(RobotController robotController) {
		myRobotController = robotController;
	}
}
