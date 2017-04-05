using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : AbstractPlayerSpawner {

    public GameObject[] pads;
    private int nextPad = 0;

    private void incrementPad() {
        if(nextPad == pads.Length - 1) {
            nextPad = 0;
        } else {
            ++nextPad;
        }
    }

    protected override Vector3 nextSpawnPos() {
        Vector3 pos = pads[nextPad].transform.position + new Vector3(0, 1, 0);
        incrementPad();
        return pos;
    }
}
