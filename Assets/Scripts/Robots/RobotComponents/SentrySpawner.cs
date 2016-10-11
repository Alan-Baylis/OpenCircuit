using UnityEngine;
using UnityEngine.Networking;

public class SentrySpawner : AbstractRobotComponent {

    public GameObject sentryModulePrefab;

    [Server]
    public SentryModule dropSentry() {
        sentryModulePrefab.GetComponent<SentryModule>().enabled = false;
        GameObject newSentry = GameObject.Instantiate(sentryModulePrefab, getController().transform.position - new Vector3(0, 1, 0), sentryModulePrefab.transform.rotation) as GameObject;
        SentryModule sentry = newSentry.GetComponent<SentryModule>();
        sentry.attachToController(getController());
        sentry.enabled = true;
        NetworkServer.Spawn(newSentry);
        return sentry;
    }
}
