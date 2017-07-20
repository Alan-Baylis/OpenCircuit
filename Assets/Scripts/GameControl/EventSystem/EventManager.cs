using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {

	public static readonly string IN_GAME_CHANNEL = "game";
	public static readonly string GAME_CONTROL_CHANNEL = "gamecontrol";

	public delegate void EventDelegate(AbstractEvent eventMessage);

	private static Dictionary<string, EventManager> channels = new Dictionary<string, EventManager>();
	public static EventManager getEventManager(string channel, bool destroyOnLoad = true) {
		if (channels.ContainsKey(channel)) {
			return channels[channel];
		} else {
			GameObject gameObject = new GameObject("EventManager-" + channel);
			if (!destroyOnLoad)
				DontDestroyOnLoad(gameObject);
			EventManager eventManager = gameObject.AddComponent<EventManager>();
			eventManager.channel = channel;
			channels[channel] = eventManager;
			return eventManager;
		}
	}

	public static EventManager getGameControlChannel() {
		return getEventManager(GAME_CONTROL_CHANNEL, false);
	}

	public static EventManager getInGameChannel() {
		return getEventManager(IN_GAME_CHANNEL);
	}

	private Dictionary <System.Type, HashSet<EventDelegate>> eventDictionary = new Dictionary<System.Type, HashSet<EventDelegate>>();
	private string channel;

	public void registerForEvent(System.Type eventType, EventDelegate action) {
		if (!eventDictionary.ContainsKey(eventType))
			eventDictionary[eventType] = new HashSet<EventDelegate>();
		eventDictionary[eventType].Add(action);
	}

	public void unregisterForEvent(System.Type eventType, EventDelegate action) {
		if (eventDictionary.ContainsKey(eventType))
			eventDictionary[eventType].Remove(action);
	}

	public static void broadcastEvent(AbstractEvent eventObject, string channel) {
		if (!channels.ContainsKey(channel))
			return;
		EventManager eventManager = channels[channel];
		HashSet<EventDelegate> eventDelegates;
		if (eventManager.eventDictionary.TryGetValue (eventObject.GetType(), out eventDelegates)) {
			foreach(EventDelegate eventDelegate in eventDelegates)
				eventDelegate(eventObject); //.Invoke ();
		}
	}

	public void broadcastEvent(AbstractEvent eventObject) {
		HashSet<EventDelegate> eventDelegates;
		if (eventDictionary.TryGetValue (eventObject.GetType(), out eventDelegates)) {
			foreach(EventDelegate eventDelegate in eventDelegates)
				eventDelegate(eventObject); //.Invoke ();
			print("sent event "+eventObject);
		}
	}

	private void OnDestroy() {
		channels.Remove(channel);
	}
}
