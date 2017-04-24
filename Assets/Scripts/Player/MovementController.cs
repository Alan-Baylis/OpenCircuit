using UnityEngine;

public abstract class MovementController : MonoBehaviour {

	public abstract void setForward(float percent);

	public abstract void setRight(float percent);

	public abstract void setSprinting(bool sprint);

	public abstract void setCrouching(bool crouch);

	public abstract void jump();

	public abstract void lockMovement();

	public abstract void unlockMovement();

	public abstract void moveToPoint(Vector3 point);

	public abstract bool isAutoMoving();
}
