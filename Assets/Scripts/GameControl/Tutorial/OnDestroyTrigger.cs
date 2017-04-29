using UnityEngine;

public class OnDestroyTrigger : MonoBehaviour {

	public Tutorial tutorial;

	private void OnDestroy() {
		tutorial.nextMessage();
	}
}
