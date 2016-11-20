﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public abstract class AbstractVisualSensor : AbstractRobotComponent {

    public float fieldOfViewAngle = 170f;           // Number of degrees, centered on forward, for the enemy sight.
    public float sightDistance = 30.0f;
	public float lookAroundInterval = 0.1f;
    public GameObject eye;

    private bool lookingEnabled = false;
    protected Dictionary<Label, SensoryInfo> targetMap = new Dictionary<Label, SensoryInfo>();
    private int visibleTargetCount = 0;

    [ServerCallback]
    public virtual void Start() {
        enableLooking();
    }

    public int getSightingCount() {
        return visibleTargetCount;
    }

    [ServerCallback]
    void OnDisable() {
        disableLooking();
    }

    [ServerCallback]
    void OnEnable() {
        enableLooking();
    }

    protected bool canSee(Transform obj) {
        Vector3 objPos = obj.position;
        bool result = false;
        if (Vector3.Distance(objPos, eye.transform.position) < sightDistance) {
            RaycastHit hit;
            Vector3 dir = objPos - eye.transform.position;
            dir.Normalize();
            float angle = Vector3.Angle(dir, eye.transform.forward);
            //			print (getController().gameObject.name);
            //			print (angle);
            if (angle < fieldOfViewAngle * 0.5f) {
                Physics.Raycast(eye.transform.position, dir, out hit, sightDistance);
                if (hit.transform == obj) {//&& Vector3.Dot (transform.forward.normalized, (objPos - eye.transform.position).normalized) > 0) {
                    result = true;
#if UNITY_EDITOR
                    if (getController().debug)
                        drawLine(eye.transform.position, hit.point, Color.green);
#endif
                } else {
                    //print("looking for: " + obj.gameObject.name);
                    //print("blocked by: " + hit.collider.gameObject.name);
#if UNITY_EDITOR
                    if (getController().debug)
                        drawLine(eye.transform.position, hit.point, Color.red);
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
        foreach (Label label in Label.visibleLabels) {
            if (label == null) {
                //TODO find a way to clean up this list
                //Label.visibleLabels.Remove(label);
                continue;
            }
            bool targetInView = hasPower && canSee(label.transform);
            if (targetInView) {
                if (!targetMap.ContainsKey(label)) {
                    Rigidbody labelRB = label.GetComponent<Rigidbody>();
                    if (labelRB != null) {
                        targetMap[label] = new SensoryInfo(label.transform.position, labelRB.velocity, System.DateTime.Now, 0);
                    } else {
                        targetMap[label] = new SensoryInfo(label.transform.position, null, System.DateTime.Now, 0);
                    }
                }
                if (targetMap[label].getSightings() == 0) {
                    registerSightingFound(label);
                }
                targetMap[label].updatePosition(label.transform.position);
            } else {
                clearSighting(label);
            }
        }
    }

    private void registerSightingFound(Label label) {
        getController().enqueueMessage(new RobotMessage(RobotMessage.MessageType.TARGET_SIGHTED, "target sighted", label.labelHandle, label.transform.position, targetMap[label].getDirection()));
        targetMap[label].addSighting();
        ++visibleTargetCount;
    }

    private void enableLooking() {
        if (!lookingEnabled) {
            InvokeRepeating("lookAround", lookAroundInterval * 1.3f, lookAroundInterval);
            lookingEnabled = true;
        }
    }

    private void disableLooking() {
        CancelInvoke("lookAround");
        lookingEnabled = false;
        if (isComponentAttached()) {
            clearSightings();
        }
    }

    private void clearSighting(Label label) {
        if (targetMap.ContainsKey(label) && targetMap[label].getSightings() == 1) {
            //print("target lost: " + label.name);
            getController().enqueueMessage(new RobotMessage(RobotMessage.MessageType.TARGET_LOST, "target lost", label.labelHandle, targetMap[label].getPosition(), targetMap[label].getDirection()));
            targetMap[label].removeSighting();
            --visibleTargetCount;
        }
    }

    private void clearSightings() {
        foreach (Label sighting in targetMap.Keys) {
            clearSighting(sighting);
        }
        targetMap.Clear();
    }

#if UNITY_EDITOR

    private List<GameObject> lines = new List<GameObject>();
    private LineRenderer myLineRenderer;
    private int size; //Total number of points in circle
    private float theta_scale = 0.01f;        //Set lower to add more points


	private LineRenderer lineRenderer { get {
			if (this.getController().debug && myLineRenderer == null) {
				float sizeValue = 2f * Mathf.PI / theta_scale;
				size = (int)sizeValue;
				size++;
				myLineRenderer = getController().gameObject.GetComponent<LineRenderer>();
				if (myLineRenderer == null) {
					myLineRenderer = getController().gameObject.AddComponent<LineRenderer>();
				}
				myLineRenderer.material = new Material(Shader.Find("Particles/Additive"));
				myLineRenderer.SetWidth(0.02f, 0.02f); //thickness of line
				myLineRenderer.SetVertexCount(size);
			}
			return myLineRenderer;
		}
	}


    protected void clearLines() {
        foreach (GameObject line in lines) {
            Destroy(line);
        }
        lines.Clear();
    }

    [ServerCallback]
    void Update() {
        if (this.isComponentAttached() && getController().debug) {
            clearCircle();
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

        for (int i = 0; i < size; i++) {
            theta += (theta_scale);
            float x = radius * Mathf.Cos(theta);
            float z = radius * Mathf.Sin(theta);
            x += gameObject.transform.position.x;
            z += gameObject.transform.position.z;
            pos = new Vector3(x, transform.position.y, z);
            lineRenderer.SetPosition(i, pos);
        }
    }

    protected void drawLine(Vector3 start, Vector3 end, Color color) {
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
