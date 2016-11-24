using UnityEngine;
using System.Collections.Generic;

public class ActionCatalog {

	private static Dictionary<TagEnum, List<System.Type>> myAvailableActionsMap;

	public static Dictionary<TagEnum, List<System.Type>> availableActionsMap { get {
			if (myAvailableActionsMap == null) {
				myAvailableActionsMap = new Dictionary<TagEnum, List<System.Type>>();
				System.Type[] types = System.Reflection.Assembly.GetAssembly(typeof(EndeavourFactory)).GetTypes();
				foreach (System.Type type in types) {
					if (!type.IsAbstract && type.IsClass && type.IsSubclassOf(typeof(EndeavourFactory))) {
						System.Reflection.MethodInfo [] methods = type.GetMethods();
						List<TagEnum> requiredTags = (List<TagEnum>)type.GetMethod("getRequiredTags").Invoke(null, null);
						foreach (TagEnum tagType in requiredTags) {
							addEntry(tagType, type);
						}
					}
				}
			}
			return myAvailableActionsMap;
		}
	}

	private static void addEntry(TagEnum tagType, System.Type systemType) {
		if (myAvailableActionsMap.ContainsKey(tagType)) {
			myAvailableActionsMap[tagType].Add(systemType);
		} else {
			List<System.Type> systemTypes = new List<System.Type>();
			systemTypes.Add(systemType);
			myAvailableActionsMap.Add(tagType, systemTypes);
		}
	}

}
