using UnityEngine;
using System.Collections;

[System.Serializable]
public class SentryDropPoint : EndeavourFactory {

    [System.NonSerialized]
    public GameObject sentryModule;

    [System.NonSerialized]
    public SentryModule sentryModulePrefab;
    private string sentryPrefabPath;

    public override Endeavour constructEndeavour(RobotController controller) {
       return new DropSentryAction(this, controller, goals, parent.labelHandle);
    }

    public SentryModule getSentryModulePrefab() {
        if (sentryModulePrefab == null && sentryPrefabPath != null) {
            sentryModulePrefab = ObjectReferenceManager.get().fetchReference<SentryModule>(sentryPrefabPath);
        }
        return sentryModulePrefab;
    }

#if UNITY_EDITOR
    public override void doGUI() {
        base.doGUI();
        sentryModulePrefab = (SentryModule)UnityEditor.EditorGUILayout.ObjectField(getSentryModulePrefab(), typeof(SentryModule), true);
        if (sentryModulePrefab != null) {
            ObjectReferenceManager.get().deleteReference(sentryPrefabPath);
            sentryPrefabPath = ObjectReferenceManager.get().addReference(sentryModulePrefab);
        } else {
            ObjectReferenceManager.get().deleteReference(sentryPrefabPath);
            sentryPrefabPath = null;
        }
    }
#endif
}
