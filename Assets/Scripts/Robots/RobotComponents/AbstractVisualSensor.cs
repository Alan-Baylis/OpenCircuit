using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public abstract class AbstractVisualSensor : AbstractRobotComponent {

    public float fieldOfViewAngle = 170f;           // Number of degrees, centered on forward, for the enemy sight.
    public float sightDistance = 30.0f;

    private bool lookingEnabled = false;
    protected Dictionary<Label, SensoryInfo> targetMap = new Dictionary<Label, SensoryInfo>();

    [ServerCallback]
    public virtual void Start() {
#if UNITY_EDITOR
        if (this.getController().debug) {
            float sizeValue = 2f * Mathf.PI / theta_scale;
            size = (int)sizeValue;
            size++;
            lineRenderer = getController().gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            lineRenderer.SetWidth(0.02f, 0.02f); //thickness of line
            lineRenderer.SetVertexCount(size);
        }
#endif
        enableLooking();
    }

    [ServerCallback]
    void OnDisable() {
        disableLooking();
    }

    [ServerCallback]
    void OnEnable() {
        enableLooking();
    }

    protected abstract bool canSee(Transform transform);

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
    }

    private void enableLooking() {
        if (!lookingEnabled) {
            InvokeRepeating("lookAround", 0.5f, .1f);
            lookingEnabled = true;
        }
    }

    private void disableLooking() {
        CancelInvoke("lookAround");
        lookingEnabled = false;
        clearSightings();
    }

    private void clearSighting(Label label) {
        if (targetMap.ContainsKey(label) && targetMap[label].getSightings() == 1) {
            //print("target lost: " + label.name);
            getController().enqueueMessage(new RobotMessage(RobotMessage.MessageType.TARGET_LOST, "target lost", label.labelHandle, targetMap[label].getPosition(), targetMap[label].getDirection()));
            targetMap[label].removeSighting();
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
    private LineRenderer lineRenderer;
    private int size; //Total number of points in circle
    private float theta_scale = 0.01f;        //Set lower to add more points

    protected void clearLines() {
        foreach (GameObject line in lines) {
            Destroy(line);
        }
        lines.Clear();
    }

    [ServerCallback]
    void Update() {
        if (getController().debug) {
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
