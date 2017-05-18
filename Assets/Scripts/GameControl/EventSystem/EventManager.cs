using System.Collections.Generic;

public class EventManager {

	public delegate void EventDelegate(AbstractEvent eventMessage);

	private static Dictionary <System.Type, HashSet<EventDelegate>> eventDictionary = new Dictionary<System.Type, HashSet<EventDelegate>>();

	public static void registerForEvent(System.Type eventType, EventDelegate action) {
		if (!eventDictionary.ContainsKey(eventType))
			eventDictionary[eventType] = new HashSet<EventDelegate>();
		eventDictionary[eventType].Add(action);
	}

	public static void unregisterForEvent(System.Type eventType, EventDelegate action) {
		if (eventDictionary.ContainsKey(eventType))
			eventDictionary[eventType].Remove(action);
	}

	public static void broadcastEvent(AbstractEvent eventObject) {
		HashSet<EventDelegate> eventDelegates = null;
		if (eventDictionary.TryGetValue (eventObject.GetType(), out eventDelegates)) {
			foreach(EventDelegate eventDelegate in eventDelegates)
				eventDelegate(eventObject); //.Invoke ();
		}
	}
}
