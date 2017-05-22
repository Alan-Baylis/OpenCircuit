using UnityEngine;

public class PlayerCamera : MonoBehaviour {

	void Start() {
		Camera camera = GetComponentInChildren<Camera>();
		Player player = GetComponentInParent<Player>();
		GlobalConfig.globalConfig.cameraManager.addCamera(player.clientController, camera);
		if (player.isLocalPlayer) {
			GlobalConfig.globalConfig.cameraManager.usePlayerCam(camera);
		}
	}

}
