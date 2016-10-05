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

	public void notifyListenersTargetFound(LabelHandle target) {
		for (int i = 0; i < listeners.Count; i++) {
			listeners[i].notifySighting(target);
		}
	}

	public void notifyListenersTargetLost(LabelHandle target) {
		for (int i = 0; i < listeners.Count; i++) {
			listeners[i].notifySightingLost(target);
		}
	}

	public void addUpdateListener(MentalModelUpdateListener listener) {
		listeners.Add (listener);
	}

    private void registerTags(LabelHandle handle) {
        List<TagEnum> tags = handle.getTagsTypes();
        foreach (TagEnum tag in tags) {
            if (knownTags.ContainsKey(tag)) {
                knownTags[tag].Add(handle.getTag(tag));
            } else {
                List<Tag> tagList = new List<Tag>();
                tagList.Add(handle.getTag(tag));
                knownTags[tag] = tagList;
            }
        }
    }

    private void unregisterTags(LabelHandle handle) {
        List<TagEnum> tags = handle.getTagsTypes();
        foreach (TagEnum tag in tags) {
            if (knownTags.ContainsKey(tag)) {
                knownTags[tag].Remove(handle.getTag(tag));
            }
        }
    }
}