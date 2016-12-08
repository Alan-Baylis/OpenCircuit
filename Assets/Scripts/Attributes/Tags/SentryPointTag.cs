using UnityEngine;
using System.Collections;

[System.Serializable]
public class SentryPointTag : Tag {

	[System.NonSerialized]
	public SentryModule sentryModule;

	public SentryPointTag(float severity, LabelHandle labelHandle) : base (TagEnum.SentryPoint, severity, labelHandle) {

	}

}
