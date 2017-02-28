using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingAnimation : MonoBehaviour {

	private Light light = null;
	private Vector3 startPos = new Vector3(18.06f, 1.77f, 4.41f); //(18.06f, 1.77f, 4.41f), (18.06f, 3f, 4.41f)
	private float movementRate = 10f;
	private float endXPos = -32f;
	
	void Start () {
		GameObject lightGameObject = new GameObject("light");
		light = lightGameObject.AddComponent<Light>();
		lightGameObject.transform.position = startPos;
		light.range = 20;
		light.intensity = 1.1f;
	}

	// Update is called once per frame
	void FixedUpdate () {
		light.transform.position += new Vector3(-movementRate*Time.deltaTime, 0f, 0f);
		if (light.transform.position.x < endXPos) {
			light.transform.position = startPos;
		}
	}
}
