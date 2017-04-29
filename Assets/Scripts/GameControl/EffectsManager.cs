using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour {

	public int maxEffects = 500;

	private Dictionary<AbstractEffectController, Queue<AbstractEffectController>> effectsMap = new Dictionary<AbstractEffectController, Queue<AbstractEffectController>>();

	public void spawnEffect(AbstractEffectController effectPrefab, Vector3 position, Vector3 direction) {
		spawnEffect(effectPrefab, position, Quaternion.LookRotation(direction));
	}

	public void spawnEffect(AbstractEffectController effectPrefab, Vector3 position, Quaternion direction) {
		if (effectPrefab == null) {
#if UNITY_EDITOR
			print("EMPTY EFFECT");
#endif
			return;
		}
		if (effectsMap.ContainsKey(effectPrefab)) {
			Queue<AbstractEffectController> effectQueue = effectsMap[effectPrefab];
			AbstractEffectController effectController = effectQueue.Peek();
			if (effectController.effectFinished()) {
				useEffect(effectController, position, direction);
				effectQueue.Dequeue();
				effectQueue.Enqueue(effectController);
			} else if (effectQueue.Count < maxEffects) {
				useEffect(createEffect(effectPrefab), position, direction);
			} else {
				useEffect(effectController, position, direction);
				effectQueue.Dequeue();
				effectQueue.Enqueue(effectController);
			}
		} else {
			useEffect(createEffect(effectPrefab), position, direction);

		}
	}

	private AbstractEffectController createEffect(AbstractEffectController effectPrefab) {
		AbstractEffectController newEffect = Instantiate(effectPrefab);
		if (!effectsMap.ContainsKey(effectPrefab)) {
			effectsMap[effectPrefab] = new Queue<AbstractEffectController>();
		}
		effectsMap[effectPrefab].Enqueue(newEffect);
		return newEffect;
	}

	private void useEffect(AbstractEffectController effectController, Vector3 position, Quaternion rotation) {
		effectController.transform.position = position;
		effectController.transform.rotation = rotation;
		effectController.doEffects();
	}

}
