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
		if (!stale && knownTags.ContainsKey(type)) {
			return knownTags[type];
		} else if (stale && previouslyKnownTags.ContainsKey(type)){
			return previouslyKnownTags[type];
		}
		else return new List<Tag>();
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

		foreach(Tag tag in target.getTags()) {
			notifyListenersTagAdded(tag);
		}
    }

    private void notifyListenersTargetFound(LabelHandle target) {
		foreach (Tag tag in target.getTags()) {
			notifyListenersTagRemoved(tag);
		}

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
            if (knownTags.ContainsKey(tag)) {
                knownTags[tag].Add(handle.getTag(tag));
				//Debug.Log("registering tag: " + handle.getTag(tag).GetType() + " as " + tag.ToString());
			} else {
                List<Tag> tagList = new List<Tag>();
                tagList.Add(handle.getTag(tag));
                knownTags[tag] = tagList;
				//Debug.Log("registering tag: " + handle.getTag(tag).GetType() + " as " + tag.ToString() );
			}
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
			if (previouslyKnownTags.ContainsKey(tag.type)) {
				previouslyKnownTags[tag.type].Add(tag);
				//Debug.Log("registering tag: " + handle.getTag(tag).GetType() + " as " + tag.ToString());
			} else {
				List<Tag> tagList = new List<Tag>();
				tagList.Add(tag);
				previouslyKnownTags[tag.type] = tagList;
				//Debug.Log("registering tag: " + handle.getTag(tag).GetType() + " as " + tag.ToString() );
			}
		}
    }
}