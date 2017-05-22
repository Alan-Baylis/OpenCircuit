using UnityEngine;
using UnityEngine.Networking;

public class SceneInitializer : MonoBehaviour {

    public Menu menuPrefab;
    public NetworkManager networkManagerPrefab;
    public SceneLoader sceneLoaderPrefab;
    public NetworkController networkControllerPrefab;

	void Start () {
	    if (NetworkManager.singleton == null) {
	        Instantiate(networkManagerPrefab.gameObject, Vector3.zero, Quaternion.identity);
	    }
	    if (Menu.menu == null) {
	        Instantiate(menuPrefab, Vector3.zero, Quaternion.identity).activeAtStart = false;
	    }
	    if (SceneLoader.sceneLoader == null) {
	        Instantiate(sceneLoaderPrefab, Vector3.zero, Quaternion.identity);
	    }
	    if (NetworkController.networkController == null) {
	        Instantiate(networkControllerPrefab, Vector3.zero, Quaternion.identity);
	    }
	}
}
