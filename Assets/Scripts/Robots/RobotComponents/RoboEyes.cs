using UnityEngine;
using UnityEngine.Networking;

[AddComponentMenu("Scripts/Robot/Robo Eyes")]
public class RoboEyes : AbstractVisualSensor {

    public Light eyeLight;
	private LaserProjector scanner;
	[SyncVar(hook = "changeEyeColor")]
	private Color eyeColor;

	// Use this for initialization
	[ServerCallback]
	public override void Start () {
        base.Start();
	    TeamId teamIdComponent = getController().GetComponent<TeamId>();
	    if (teamIdComponent.enabled) {
			eyeColor = teamIdComponent.team.config.color;
	    }
		scanner = GetComponent<LaserProjector>();
	}

	public GameObject lookAt(Vector3 position) {
		if(powerSource.hasPower(Time.deltaTime)) {
			Vector3 direction = position - transform.position;
			RaycastHit hitInfo;
			if(Physics.Raycast(transform.position, direction, out hitInfo, direction.magnitude)) {
#if UNITY_EDITOR
				if (getController().debug)
					drawLine(transform.position, hitInfo.point, Color.green);
#endif
				return hitInfo.collider.gameObject;
			}
		}

#if UNITY_EDITOR
		if (getController().debug)
			drawLine(transform.position, position, Color.red);
#endif
		return null;
	}

	public bool hasScanner() {
		return scanner != null;
	}

	public LaserProjector getScanner() {
		return scanner;
	}

	private void changeEyeColor(Color color) {
		eyeColor = color;
		Renderer renderer = GetComponent<Renderer>();
		if (renderer != null) {
			Material mat = renderer.material;

			mat.SetColor("_EmissionColor", color);
			mat.SetColor("_Albedo", color);
			eyeLight.color = color;
		}
	}
}
