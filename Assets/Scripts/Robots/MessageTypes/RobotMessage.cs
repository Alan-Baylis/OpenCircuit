using UnityEngine;

public class RobotMessage {

	string message;
	LabelHandle target;
	Vector3 targetPos;
	Vector3? targetVelocity;

	public RobotMessage(string message, LabelHandle target, Vector3 targetPos, Vector3? velocity) {
		this.message = message;
		this.target = target;
		this.targetPos = targetPos;
		this.targetVelocity = velocity;
	}

	public Vector3 TargetPos {
		get { return targetPos; }
		set { targetPos = value; }
	}

	public string Message {
		get {
			return message;
		}
		set {
			message = value;
		}
	}

	public LabelHandle Target {
		get {
			return target;
		}
		set {
			target = value;
		}
	}

	public Vector3? TargetVelocity {
		get { return targetVelocity; }
		set { targetVelocity = value; }
	}
}
