using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class NetworkParenter : NetworkBehaviour {

	[SyncVar(hook = "updateParentId")]
	protected NetworkInstanceId parentId;

	public override void OnStartClient() {
		base.OnStartClient();
		updateParentId(parentId);
	}

	[Server]
	public void setParentId(NetworkInstanceId id) {
		this.parentId = id;
	}

	[Client]
	protected void updateParentId(NetworkInstanceId id) {
		this.parentId = id;
		GameObject parentObject = ClientScene.FindLocalObject(id);
		if(parentObject != null) {
			transform.parent = parentObject.transform;
		}
	}
}
