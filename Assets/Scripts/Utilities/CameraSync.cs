using UnityEngine;
using System.Collections;

public class CameraSync : TransformSync {

    private Player myPlayer;

    private Player player {
        get {
            if (myPlayer == null) {
                myPlayer = GetComponentInParent<Player>();
            }
            return myPlayer;
        }

    }

    protected override void syncPosition() {
        if (player != null && !player.isLocalPlayer || isServer) {
            base.syncPosition();
        }
    }

    protected override void syncRotation() {
        if (player != null && !player.isLocalPlayer || isServer) {
            base.syncRotation();
        }
    }
}
