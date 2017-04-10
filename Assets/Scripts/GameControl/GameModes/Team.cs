using UnityEngine;
using UnityEngine.Networking;

public class Team : NetworkBehaviour {

    [SyncVar]
    public TeamData team;

    [ServerCallback]
    void Start() {
        Label label = GetComponent<Label>();
        label.setTag(new Tag(TagEnum.Team, 0, label.labelHandle));
    }
}
