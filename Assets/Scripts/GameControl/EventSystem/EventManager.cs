using System.Collections.Generic;

public class EventManager {

	public delegate void EventDelegate(AbstractEvent eventMessage);

	private static EventManager myInstance;

	private static EventManager instance {
		get {
			if (myInstance == null)
				myInstance = new EventManager();
			return myInstance;
		}
	}

	private Dictionary <System.Type, HashSet<EventDelegate>> eventDictionary = new Dictionary<System.Type, HashSet<EventDelegate>>();

	private EventManager() {

	}

	public static void clearInstance() {
		myInstance = null;
	}

	public static void registerForEvent(System.Type eventType, EventDelegate action) {
		if (!instance.eventDictionary.ContainsKey(eventType))
			instance.eventDictionary[eventType] = new HashSet<EventDelegate>();
		instance.eventDictionary[eventType].Add(action);
	}

	public static void unregisterForEvent(System.Type eventType, EventDelegate action) {
		if (instance.eventDictionary.ContainsKey(eventType))
			instance.eventDictionary[eventType].Remove(action);
	}

	public static void broadcastEvent(AbstractEvent eventObject) {
		HashSet<EventDelegate> eventDelegates;
		if (instance.eventDictionary.TryGetValue (eventObject.GetType(), out eventDelegates)) {
			foreach(EventDelegate eventDelegate in eventDelegates)
				eventDelegate(eventObject); //.Invoke ();
		}
	}
}
