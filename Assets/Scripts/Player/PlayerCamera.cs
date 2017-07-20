using UnityEngine;

public class PlayerCamera : MonoBehaviour {
	private Camera cam;
	private ClientController clientController;

	void Start() {
		cam = GetComponentInChildren<Camera>();
		clientController = GetComponentInParent<Player>().clientController;
		GlobalConfig.globalConfig.cameraManager.addCamera(clientController, cam);
		if (clientController.isLocalPlayer) {
			GlobalConfig.globalConfig.cameraManager.usePlayerCam(cam);
		}
	}

	private void OnDestroy() {
		GlobalConfig.globalConfig.cameraManager.removeCamera(clientController);
		if(clientController != null && clientController.isLocalPlayer) {
			GlobalConfig.globalConfig.cameraManager.switchCamera();
		}
	}
}
