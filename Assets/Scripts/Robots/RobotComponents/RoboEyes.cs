using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Scripts/Robot/Robo Eyes")]
public class RoboEyes : AbstractRobotComponent {

	public float fieldOfViewAngle = 170f;           // Number of degrees, centered on forward, for the enemy sight.
	public float sightDistance = 30.0f;
	
	private Dictionary<Label, SensoryInfo> targetMap = new Dictionary<Label, SensoryInfo>();

	private LaserProjector scanner;

	// Use this for initialization
	[ServerCallback]
	void Start () {
		scanner = GetComponent<LaserProjector>();
#if UNITY_EDITOR
		float sizeValue = 2f*Mathf.PI / theta_scale; 
		size = (int)sizeValue;
		size++;
		lineRenderer = getController().gameObject.AddComponent<LineRenderer>();
		lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		lineRenderer.SetWidth(0.02f, 0.02f); //thickness of line
		lineRenderer.SetVertexCount(size);
#endif
		InvokeRepeating ("lookAround", 0.5f, .03f);
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

	private bool canSee (Transform obj) {
		Vector3 objPos = obj.position;
		bool result = false;
		if (Vector3.Distance (objPos, transform.position) < sightDistance) {
			RaycastHit hit;
			Vector3 dir = objPos - transform.position;
			dir.Normalize();
			float angle = Vector3.Angle(dir, transform.forward);
//			print (getController().gameObject.name);
//			print (angle);
			if(angle < fieldOfViewAngle * 0.5f) {
				Physics.Raycast (transform.position, dir, out hit, sightDistance);
				if (hit.transform == obj ) {//&& Vector3.Dot (transform.forward.normalized, (objPos - transform.position).normalized) > 0) {
					result = true;
#if UNITY_EDITOR
					if (getController().debug)
						drawLine(transform.position, hit.point, Color.green);
#endif
				} else {
					//print("looking for: " + obj.gameObject.name);
					//print("blocked by: " + hit.collider.gameObject.name);
#if UNITY_EDITOR
					if (getController().debug)
						drawLine(transform.position, hit.point, Color.red);
#endif
					//print("lost: " + obj.gameObject.name + "obscured by: " + hit.transform.gameObject.name);
				}
			}
		}
		return result;
	}

	private void lookAround() {
#if UNITY_EDITOR
		clearLines();
#endif
		bool hasPower = (powerSource != null) && powerSource.hasPower(Time.deltaTime);
		foreach(Label label in Label.visibleLabels) {
			if(label == null) {
				//TODO find a way to clean up this list
				//Label.visibleLabels.Remove(label);
				continue;
			}
			bool targetInView = hasPower && canSee(label.transform);
			if(targetInView) {
				if(!targetMap.ContainsKey(label)) {
					Rigidbody labelRB = label.GetComponent<Rigidbody>();
					if(labelRB != null) {
						targetMap[label] = new SensoryInfo(label.transform.position, labelRB.velocity, System.DateTime.Now, 0);

					} else {
						targetMap[label] = new SensoryInfo(label.transform.position, null, System.DateTime.Now, 0);
					}
				}
				if(targetMap[label].getSightings() == 0) {
					//print("target sighted: " + label.name);
					getController().enqueueMessage(new RobotMessage(RobotMessage.MessageType.TARGET_SIGHTED, "target sighted", label.labelHandle, label.transform.position, targetMap[label].getDirection()));
					targetMap[label].addSighting();
				}
				targetMap[label].updatePosition(label.transform.position);
			} else {
				if(targetMap.ContainsKey(label) && targetMap[label].getSightings() == 1) {
					//print("target lost: " + label.name);
					getController().enqueueMessage(new RobotMessage(RobotMessage.MessageType.TARGET_LOST, "target lost", label.labelHandle, targetMap[label].getPosition(), targetMap[label].getDirection()));
					targetMap[label].removeSighting();
				}
			}
		}
	}

#if UNITY_EDITOR

	private List<GameObject> lines = new List<GameObject>();
	private LineRenderer lineRenderer;
	private int size; //Total number of points in circle
	private float theta_scale = 0.01f;        //Set lower to add more points

	private void clearLines() {
		foreach(GameObject line in lines) {
			Destroy(line);
		}
		lines.Clear();
	}

	[ServerCallback]
	void Update() {
		clearCircle();
		if(getController().debug) {
			lineRenderer.SetVertexCount(size);
			drawCircle();
		}
	}

	private void clearCircle() {
		lineRenderer.SetVertexCount(0);
	}

	private void drawCircle() {
		Vector3 pos;
		float theta = 0f;
		float radius = sightDistance;

		for(int i = 0; i < size; i++){
			theta += (theta_scale);         
		  float x = radius * Mathf.Cos(theta);
		  float	z = radius * Mathf.Sin(theta);          
		  x += gameObject.transform.position.x;
		  z += gameObject.transform.position.z;
		  pos = new Vector3(x, transform.position.y, z);
		  lineRenderer.SetPosition(i, pos);
		}
	}

	private void drawLine(Vector3 start, Vector3 end, Color color) {
		LineRenderer line = new GameObject("Line ").AddComponent<LineRenderer>();
		line.SetWidth(0.025F, 0.025F);
		line.SetColors(color, color);
		line.SetVertexCount(2);
		line.SetPosition(0, start);
		line.SetPosition(1, end);
		line.material.shader = (Shader.Find("Unlit/Color"));
		line.material.color = color;
		lines.Add(line.gameObject);
	}
#endif

}
