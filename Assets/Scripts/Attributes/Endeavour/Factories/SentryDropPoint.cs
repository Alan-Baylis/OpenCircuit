using UnityEngine;
using System.Collections;

[System.Serializable]
public class SentryDropPoint : EndeavourFactory {

    [System.NonSerialized]
    public SentryModule sentryModule;

    public override Endeavour constructEndeavour(RobotController controller) {
       return new DropSentryAction(this, controller, goals, parent.labelHandle);
    }
}
