using UnityEngine;

public class LoadingAnimation : MonoBehaviour {

	private Light sceneLight;
	private Vector3 startPos = new Vector3(18.06f, 1.77f, 4.41f); //(18.06f, 1.77f, 4.41f), (18.06f, 3f, 4.41f)
	private float movementRate = 10f;
	private float endXPos = -32f;
	
	void Start () {
		GameObject lightGameObject = new GameObject("light");
		sceneLight = lightGameObject.AddComponent<Light>();
		lightGameObject.transform.position = startPos;
		sceneLight.range = 20;
		sceneLight.intensity = 1.1f;
	}

	// Update is called once per frame
	void FixedUpdate () {
		sceneLight.transform.position += new Vector3(-movementRate*Time.deltaTime, 0f, 0f);
		if (sceneLight.transform.position.x < endXPos) {
			sceneLight.transform.position = startPos;
		}
	}
}
