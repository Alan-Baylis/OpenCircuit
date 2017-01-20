using UnityEngine;

public class RobotMessage {

	public enum MessageType {
		ACTION
	}

	string message;
	MessageType type;
	LabelHandle target;
	Vector3 targetPos;
	Vector3? targetVelocity;

	public RobotMessage(MessageType type, string message, LabelHandle target, Vector3 targetPos, Vector3? velocity) {
		this.type = type;
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

	public MessageType Type {
		get {
			return type;
		}
		set {
			type = value;
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
