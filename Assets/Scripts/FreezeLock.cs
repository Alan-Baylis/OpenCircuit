using UnityEngine;
using System.Collections;

public class FreezeLock : MonoBehaviour {

    public Player frozenPlayer;

    void OnDisable() {
        frozenPlayer.unfreeze();
    }


}
