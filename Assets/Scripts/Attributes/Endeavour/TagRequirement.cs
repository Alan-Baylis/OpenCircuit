public class TagRequirement {
	
	private TagEnum type;
	private bool stale = false;

	public TagRequirement(TagEnum type, bool stale) {
		this.type = type;
		this.stale = stale;
	}

	public TagEnum getType() {
		return type;
	}

	public bool isStale() {
		return stale;
	}
}
