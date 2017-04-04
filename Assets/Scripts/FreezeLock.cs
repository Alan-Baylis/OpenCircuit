using UnityEngine;

public class FreezeLock : MonoBehaviour {

    public Player frozenPlayer;

    void OnDisable() {
        GlobalConfig.globalConfig.gamemode.onPlayerRevive(frozenPlayer);
    }


}
