using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class AbstractPlayerSpawner : NetworkBehaviour {

    protected abstract Vector3 nextSpawnPos();

    public void respawnPlayer(ClientController client) {
        if(!client.isAlive()) {
            client.respawnPlayerAt(nextSpawnPos());
        }
    }
}
