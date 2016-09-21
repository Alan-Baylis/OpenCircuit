using UnityEngine;
using System.Collections;

[System.Serializable]
public class SentryDropPoint : EndeavourFactory {

    [System.NonSerialized]
    public GameObject sentryModule;

    [System.NonSerialized]
    public GameObject sentryModulePrefab;
    private string sentryPrefabPath = null;

    public override Endeavour constructEndeavour(RobotController controller) {
       return new DropSentryAction(this, controller, goals, parent.labelHandle);
    }

    public GameObject getSentryModulePrefab() {
        if (sentryModulePrefab == null && sentryPrefabPath != null) {
            sentryModulePrefab = ObjectReferenceManager.get().fetchReference<GameObject>(sentryPrefabPath);
        }
        return sentryModulePrefab;
    }

#if UNITY_EDITOR
    public override void doGUI() {
        base.doGUI();
        GameObject nextPrefab = (GameObject)UnityEditor.EditorGUILayout.ObjectField(getSentryModulePrefab(), typeof(GameObject), true);
        if (nextPrefab != null) {
            if (nextPrefab != getSentryModulePrefab()) {
                ObjectReferenceManager.get().deleteReference(parent, sentryPrefabPath);
                sentryPrefabPath = ObjectReferenceManager.get().addReference(parent, nextPrefab);
                sentryModulePrefab = nextPrefab;
            }
        } else if (sentryPrefabPath != null) {
            ObjectReferenceManager.get().deleteReference(parent, sentryPrefabPath);
            sentryModulePrefab = null;
            sentryPrefabPath = null;
        }
    }
#endif
}
