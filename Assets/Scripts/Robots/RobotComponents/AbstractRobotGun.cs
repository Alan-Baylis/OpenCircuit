using UnityEngine;
using UnityEngine.Networking;

public abstract class AbstractRobotGun : AbstractRobotComponent {

	public GenericRifle rifle;

	protected LabelHandle currentTarget;

	public LabelHandle target {
		get { return currentTarget; }
	}

	// Update is called once per frame
	[ServerCallback]
	void Update () {
		if (currentTarget != null && rifle.targetInRange(currentTarget.getPosition())) {
			rifle.firing = true;
		} else {
			rifle.firing = false;
		}
	}

	[ServerCallback]
	void FixedUpdate() {
//		double startTime = Time.realtimeSinceStartup;
		if (currentTarget != null) {
			trackTarget(currentTarget.getPosition());
		} else {
			trackTarget(getRestPosition());
		}
//		double endTime = Time.realtimeSinceStartup;
//		getController().getExecutionTimer().addTime(endTime-startTime);
	}

	public override void release() {
		currentTarget = null;
		rifle.firing = false;
	}

	public void setTarget(LabelHandle handle) {
		currentTarget = handle;
	}

	protected virtual Vector3 getRestPosition() {
		return transform.position + getController().transform.forward;
	}

	public bool targetObstructed(LabelHandle handle) {
		Vector3 objPos = handle.getPosition();
		bool result = false;
		Vector3 rayStart = transform.position;
		Vector3 dir = objPos - rayStart;
		RaycastHit hit;
		Physics.Raycast(rayStart, dir, out hit, Vector3.Distance(rayStart, objPos));
//		RaycastHit[] hits = Physics.RaycastAll(rayStart, dir, (Vector3.Distance(rayStart, objPos)));
//		foreach (RaycastHit hit in hits) {
			Transform hitTransform = hit.transform;
//			if (hitTransform == handle.label.transform || hitTransform.root == handle.label.transform) {
//				continue;
//			}
//			if (hitTransform.root != transform.root) {
//				result = true;
//				break;
//			}
//		}
		return !(hitTransform.root == handle.label.transform || hitTransform == handle.label.transform);
	}

	public override System.Type getComponentArchetype() {
		return typeof(AbstractRobotGun);
	}

	protected abstract void trackTarget(Vector3 pos);

}
