using UnityEngine;

[System.Serializable]
public struct EffectSpec {

	public GameObject prefab;
	public float lifetime;

	public GameObject spawn(Vector3 location) {
		if (prefab == null)
			return spawn(location, Quaternion.identity);
		return spawn(location, prefab.transform.rotation);
	}

	public GameObject spawn(Vector3 location, Vector3 direction) {
		return spawn(location, Quaternion.LookRotation(direction));
	}

	public GameObject spawn(Vector3 location, Quaternion direction) {
		if (prefab == null) {
			MonoBehaviour.print("EMPTY EFFECT");
			return null;
		}
		GameObject effect = MonoBehaviour.Instantiate(prefab, location, direction) as GameObject;
		effect.gameObject.hideFlags |= HideFlags.HideAndDontSave;
		if (lifetime > 0)
			MonoBehaviour.Destroy(effect.gameObject, lifetime);
		return effect;
	}
}
