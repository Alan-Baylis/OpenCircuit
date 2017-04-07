using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

	private List<CamInfo> availableCameras = new List<CamInfo>();
	private Dictionary<ClientController, CamInfo> cameraMap = new Dictionary<ClientController, CamInfo>();

	private int nextCameraIndex;
	private CamInfo sceneCamera;
	private CamInfo lastCamera;

	void Start() {
		Camera[] cams = FindObjectsOfType<Camera>();
		foreach (Camera cam in cams) {
			if (cam.tag == "SceneCamera") {
				sceneCamera = new CamInfo(null, cam);
				lastCamera = sceneCamera;
				availableCameras.Add(sceneCamera);
				break;
			}
		}
	}

	public void addCamera(ClientController controller, Camera cam) {
		CamInfo newCamInfo = new CamInfo(controller, cam);
		cameraMap[controller] = newCamInfo;
		availableCameras.Add(newCamInfo);
	}

	public void removeCamera(ClientController controller) {
		availableCameras.Remove(cameraMap[controller]);
		cameraMap.Remove(controller);
		if (GlobalConfig.globalConfig.localPlayerDead && lastCamera.controller == controller) {
			nextCameraIndex = 0;
			switchCamera();
		}
	}

	public void switchCamera() {
		if (lastCamera != null)
			disableCamera(lastCamera.cam);
		lastCamera = availableCameras[nextCameraIndex];
		enableCamera(lastCamera.cam);

		incrementCamera();
	}

	public void usePlayerCam(Camera camera) {
		if (lastCamera != null) {
			disableCamera(lastCamera.cam);
		}
		enableCamera(camera);
		nextCameraIndex = 0;
	}

	private void enableCamera(Camera cam) {
		cam.enabled = true;
		cam.GetComponent<AudioListener>().enabled = true;
	}

	private void disableCamera(Camera cam) {
		cam.enabled = false;
		cam.GetComponent<AudioListener>().enabled = false;
	}

	private void incrementCamera() {
		if(nextCameraIndex >= availableCameras.Count - 1) {
			nextCameraIndex = 0;
		} else {
			nextCameraIndex++;
		}
	}

	private class CamInfo {
		public readonly ClientController controller;
		public readonly Camera cam;

		public CamInfo(ClientController controller, Camera cam) {
			this.controller = controller;
			this.cam = cam;
		}
	}
}
