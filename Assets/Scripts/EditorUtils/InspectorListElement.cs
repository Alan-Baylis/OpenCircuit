using UnityEngine;

public interface InspectorListElement {

#if UNITY_EDITOR
	InspectorListElement doListElementGUI(GameObject parent);
#endif

}
