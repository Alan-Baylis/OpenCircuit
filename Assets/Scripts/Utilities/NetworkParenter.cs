using UnityEngine;
using UnityEngine.Networking;

public class NetworkParenter : NetworkBehaviour {

	public bool setPositionAndRotation = false;
	public PositionAndRotation positionAndRotation;

	[SyncVar(hook = "updateParentId")]
	protected NetworkInstanceId parentId;

	public override void OnStartClient() {
		base.OnStartClient();
	    if (!isServer) {
	        updateParentId(parentId);
	    }
	}

	[Server]
	public void setParentId(NetworkInstanceId id) {
		this.parentId = id;
	}

	public NetworkInstanceId getParentId() {
		return parentId;
	}

	[Client]
	protected void updateParentId(NetworkInstanceId id) {
		this.parentId = id;
		GameObject parentObject = ClientScene.FindLocalObject(id);
		if(parentObject != null) {
			transform.parent = parentObject.transform;
			if (setPositionAndRotation) {
				transform.localPosition = positionAndRotation.position;
				transform.localEulerAngles = positionAndRotation.rotation;
			}
		}
	}

	[System.Serializable]
	public struct PositionAndRotation {
		public Vector3 position;
		public Vector3 rotation;
	}
}
