using UnityEngine;
using System.Collections.Generic;

public class MentalModel {

	private Dictionary<LabelHandle, SensoryInfo> targetSightings = new Dictionary<LabelHandle, SensoryInfo>();
	private Dictionary<LabelHandle, SensoryInfo> staleTargetSightings = new Dictionary<LabelHandle, SensoryInfo>();
	private Dictionary<TagEnum, List<Tag>> knownTags = new Dictionary<TagEnum, List<Tag>>();
	private Dictionary<TagEnum, List<Tag>> previouslyKnownTags = new Dictionary<TagEnum, List<Tag>>();

	private List<MentalModelUpdateListener> listeners = new List<MentalModelUpdateListener> ();

	public void addSighting(LabelHandle target, Vector3 position, Vector3? direction) {
		if (!target.hasTags()) {
			return;
		}
		if (targetSightings.ContainsKey (target)) {
			SensoryInfo info = targetSightings[target];
			info.addSighting();
			info.updateInfo(target);
		} else {
			if (staleTargetSightings.ContainsKey(target)) {
				SensoryInfo info = staleTargetSightings[target];
                targetSightings.Add(target, info);
				staleTargetSightings.Remove(target);
				info.addSighting();
			} else {
				targetSightings[target] = new SensoryInfo(position, direction, System.DateTime.Now, target.getTags(), 1);
			}
			registerTags(target);
			notifyListenersTargetLost(target);
			notifyListenersTargetFound(target);
		}
	}

	public void removeSighting(LabelHandle target, Vector3 position, Vector3? direction) {
		if (targetSightings.ContainsKey (target)) {
			SensoryInfo info = targetSightings[target];

			info.removeSighting();
			if (info.getSightings() < 1) {
                unregisterTags(target);
				targetSightings.Remove(target);
				staleTargetSightings.Add(target, info);
				notifyListenersTargetLost (target);
				if (!target.isBacked || target.label != null)
					notifyListenersTargetFound(target);
			}
			info.updateInfo(position, System.DateTime.Now, direction);
		} else {
			//Realistically we should never get here. This case is stupid.
			targetSightings[target] = new SensoryInfo(position, direction, System.DateTime.Now, null, 0);
			notifyListenersTargetLost (target);
			Debug.LogWarning("Target '" + target.getName() + "' that was never found has been lost. Shenanigans?");
		}
	}

	public bool canSee(LabelHandle target) {
		return targetSightings.ContainsKey(target) && targetSightings[target].getSightings() > 0;
	}

	public System.Nullable<Vector3> getLastKnownPosition(LabelHandle target) {
		if (targetSightings.ContainsKey(target)) {
			return targetSightings[target].getPosition();
		} else if (staleTargetSightings.ContainsKey(target)) {
			return staleTargetSightings[target].getPosition();
		}
		return null;
	}

	public System.DateTime? getLastSightingTime(LabelHandle target) {
		if(targetSightings.ContainsKey(target)) {
			return targetSightings[target].getSightingTime();
		} else if (staleTargetSightings.ContainsKey(target)) {
			return staleTargetSightings[target].getSightingTime();
		}
		return null;
	}

	public void addUpdateListener(MentalModelUpdateListener listener) {
		listeners.Add (listener);
	}

    public List<Tag> getTagsOfType (TagEnum type, bool stale) {
		List<Tag> tags;
		if (stale) {
			previouslyKnownTags.TryGetValue(type, out tags);
		} else {
			knownTags.TryGetValue(type, out tags);
		}
		return tags != null ? tags : new List<Tag>();
    }

    public List<TagEnum> getKnownTagTypes() {
        List<TagEnum> tags = new List<TagEnum>();
        foreach (TagEnum tagEnum in knownTags.Keys) {
            tags.Add(tagEnum);
        }
        return tags;
    }

    private void notifyListenersTargetLost(LabelHandle target) {
	    foreach (Tag tag in target.getTags()) {
		    notifyListenersTagRemoved(tag);
	    }
    }

    private void notifyListenersTargetFound(LabelHandle target) {
		foreach (Tag tag in target.getTags()) {
            notifyListenersTagAdded(tag);
        }
	}

    private void notifyListenersTagRemoved(Tag tag) {
        for (int i = 0; i < listeners.Count; i++) {
            listeners[i].removeTag(tag);
        }
    }

    private void notifyListenersTagAdded(Tag tag) {
        for (int i = 0; i < listeners.Count; i++) {
            listeners[i].addTag(tag);
        }
    }

	private void registerTags(LabelHandle handle) {
		// Clean out all stale tags from this LabelHandle before registering new ones
		SensoryInfo info;
		staleTargetSightings.TryGetValue(handle, out info);
		if (info != null) {
			foreach (Tag tag in info.getAttachedTags()) {
				List<Tag> staleTags;
				previouslyKnownTags.TryGetValue(tag.type, out staleTags);
				if (staleTags != null) {
					staleTags.Remove(tag);
				}
			}
		}

		List<TagEnum> tags = handle.getTagTypes();
		foreach (TagEnum tag in tags) {
			addTag (knownTags, handle.getTag(tag));
		}
	}

	private void unregisterTags(LabelHandle handle) {
		List<TagEnum> tags = handle.getTagTypes();
		foreach (TagEnum tag in tags) {
			if (knownTags.ContainsKey(tag)) {
				knownTags[tag].Remove(handle.getTag(tag));
			}
		}

		foreach (Tag tag in targetSightings[handle].getAttachedTags()) {
			addTag(previouslyKnownTags, tag);
		}
    }

	private static void addTag(Dictionary<TagEnum, List<Tag>> tags, Tag tag) {
		if (tags.ContainsKey(tag.type)) {
			tags[tag.type].Add(tag);
		} else {
			List<Tag> tagList = new List<Tag>();
			tagList.Add(tag);
			tags[tag.type] = tagList;
		}
	}
}