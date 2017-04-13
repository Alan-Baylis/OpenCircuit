using UnityEngine;
using UnityEngine.Networking;

public class CheckPoint : AbstractPlayerSpawner {

	public GameObject[] pads;
	private int nextPad = 0;

	[ServerCallback]
	public void OnTriggerEnter(Collider other) {
		Player player = other.GetComponent<Player>();
		if(player != null) {
			ClientController [] controllers = FindObjectsOfType<ClientController>();
			foreach(ClientController controller in controllers) {
				if(pads != null) {
					if(pads[nextPad] != null) {
                        respawnPlayer(controller);
					} else {
						Debug.LogWarning("Null pad attached to checkpoint: '" + gameObject.name + "' in position " + nextPad + "!");
					}
				} else {
					Debug.LogWarning("No viable pads attached to checkpoint: '"+ gameObject.name+"'!");
				}
			}
		}
	}

	private void incrementPad() {
		if(nextPad == pads.Length - 1) {
			nextPad = 0;
		} else {
			++nextPad;
		}
	}

    public override Vector3 nextSpawnPos() {
        Vector3 pos = pads[nextPad].transform.position + new Vector3(0, 1, 0);
        incrementPad();
        return pos;
    }
}
