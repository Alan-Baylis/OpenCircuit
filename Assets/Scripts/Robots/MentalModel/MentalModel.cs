using UnityEngine;
using System.Collections.Generic;

public class MentalModel {

	Dictionary<LabelHandle, SensoryInfo> targetSightings = new Dictionary<LabelHandle, SensoryInfo>();
    Dictionary<TagEnum, List<Tag>> knownTags = new Dictionary<TagEnum, List<Tag>>();

	List<MentalModelUpdateListener> listeners = new List<MentalModelUpdateListener> ();

	public void addSighting(LabelHandle target, Vector3 position, Vector3? direction) {
		if (targetSightings.ContainsKey (target)) {
			SensoryInfo info = targetSightings[target];
			
			if (info.getSightings() == 0) {
				// We have to increment the sighting count before we notify listeners
				info.addSighting();
				notifyListenersTargetFound(target);
			}
			else {
				// Keep this. See above comment
				info.addSighting();
			}
			info.updatePosition(position);
			info.updateDirection(direction);
		} else {
			targetSightings[target] = new SensoryInfo(position, direction, System.DateTime.Now, 1);
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
				notifyListenersTargetLost (target);
			}
			info.updatePosition(position);
			info.updateTime(System.DateTime.Now);
		} else {
			//Realistically we should never get here. This case is stupid.
			targetSightings[target] = new SensoryInfo(position, direction, System.DateTime.Now, 0);
			notifyListenersTargetLost (target);
			Debug.LogWarning("Target '" + target.getName() + "' that was never found has been lost. Shenanigans?");
		}
	}

	public bool canSee(LabelHandle target) {
		return targetSightings.ContainsKey(target) && targetSightings[target].getSightings() > 0;
	}

	public bool knowsTarget(LabelHandle target) {
		return targetSightings.ContainsKey(target);
	}

	public System.Nullable<Vector3> getLastKnownPosition(LabelHandle target) {
		if (targetSightings.ContainsKey(target)) {
			return targetSightings[target].getPosition();
		}
		return null;
	}

	public System.DateTime? getLastSightingTime(LabelHandle target) {
		if(targetSightings.ContainsKey(target)) {
			return targetSightings[target].getSightingTime();
		}
		return null;
	}

	public void addUpdateListener(MentalModelUpdateListener listener) {
		listeners.Add (listener);
	}

    public List<Tag> getTagsOfType (TagEnum type) {
        if (knownTags.ContainsKey(type))
            return knownTags[type];
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
    }
}