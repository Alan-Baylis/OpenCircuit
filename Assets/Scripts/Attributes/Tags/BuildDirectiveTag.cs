[System.Serializable]
public class BuildDirectiveTag : Tag {

	[System.NonSerialized]
	public ClientController owner;

	public BuildDirectiveTag(ClientController owner, float severity, LabelHandle handle) : base(TagEnum.BuildDirective, severity, handle) {
		this.owner = owner;
	}
}
