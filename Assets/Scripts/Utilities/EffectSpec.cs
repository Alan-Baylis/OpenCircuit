using UnityEngine;

[System.Serializable]
public struct EffectSpec {

	public Transform prefab;
	public float lifetime;

	public Transform spawn(Vector3 location) {
		return spawn(location, Quaternion.identity);
	}

	public Transform spawn(Vector3 location, Vector3 direction) {
		return spawn(location, Quaternion.LookRotation(direction));
	}

	public Transform spawn(Vector3 location, Quaternion direction) {
		if (prefab == null) {
			MonoBehaviour.print("EMPTY EFFECT");
			return null;
		}
		Transform effect = (Transform)GameObject.Instantiate(prefab, location, direction);
		effect.gameObject.hideFlags |= HideFlags.HideAndDontSave;
		if (lifetime > 0)
			MonoBehaviour.Destroy(effect.gameObject, lifetime);
		return effect;
	}
}
