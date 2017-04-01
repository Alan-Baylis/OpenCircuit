public struct TagRequirement {
	
	public readonly TagEnum type;
	public readonly bool stale;

	public TagRequirement(TagEnum type, bool stale=false) {
		this.type = type;
		this.stale = stale;
	}
}
