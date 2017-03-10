using UnityEngine;

public class BodyController : MonoBehaviour {

	public float minRotationDiff;
	public float rotationPercent;
	public float maxDegreesPerSecond;
	public float minDegreesPerSecond;

	protected bool rotating;

	private Player myPlayer;
	private ChassisController myLegs;

	protected Player player {
		get { return myPlayer ?? (myPlayer = transform.GetComponentInParent<Player>()); }
	}

	protected ChassisController legs {
		get { return myLegs ?? (myLegs = GetComponent<ChassisController>()); }
	}


	public void Update () {
		Quaternion camRotation = Quaternion.Euler(0, player.cam.transform.localEulerAngles.y, 0);
		Quaternion rotation = Quaternion.Euler(transform.localEulerAngles);
		float diff = Quaternion.Angle(rotation, camRotation);
		print(rotating + ", " + diff);
		if (rotating) {
			float rotationChange = Mathf.Max(minDegreesPerSecond *Time.deltaTime,
				Mathf.Min(diff * rotationPercent, maxDegreesPerSecond * Time.deltaTime));
			transform.localEulerAngles = Quaternion.RotateTowards(rotation, camRotation, rotationChange).eulerAngles;
			if (diff < rotationChange)
				rotating = false;
		} else {
			if (diff > minRotationDiff)
				rotating = true;
		}
	}
}
