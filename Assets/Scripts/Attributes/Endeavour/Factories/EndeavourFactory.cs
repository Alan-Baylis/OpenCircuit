using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public abstract class EndeavourFactory : InspectorListElement {

	[System.NonSerialized] public List<TagRequirement> requiredTagsCache;

	public List<Goal> goals = new List<Goal> ();
    public float maxMobBenefit;
    public int optimalMobSize = 1;
    public float mobCostPerRobot = 10;

    private bool status;
	private int size;

	private static string[] typeNames;

	private static System.Type[] eTypes;

	public static System.Type[] types { get {
			if (eTypes == null) {
				System.Type[] ts = System.Reflection.Assembly.GetAssembly(typeof(EndeavourFactory)).GetTypes();
				List<System.Type> pairedTypes = new List<System.Type>();
				System.Type targetType = typeof(EndeavourFactory);
				foreach (System.Type t in ts) {
					if (t.IsSubclassOf(targetType) && !t.IsAbstract)
						pairedTypes.Add(t);
				}
				eTypes = pairedTypes.ToArray();
			}
			return eTypes;
		}
	}

	public static readonly GoalEnum[] goalEnums;

	static EndeavourFactory() {
		System.Array values = System.Enum.GetValues(typeof(GoalEnum));
		List<GoalEnum> typeList = new List<GoalEnum>();
		foreach(object obj in values) {
			typeList.Add((GoalEnum)obj);
		}
		goalEnums = typeList.ToArray();
	}

	public Endeavour constructEndeavour(RobotController controller, List<Tag> tags) {
		Dictionary<TagEnum, Tag> tagMap = new Dictionary<TagEnum, Tag>();
		foreach (Tag tag in tags) {
			tagMap.Add(tag.type, tag);
		}
		return createEndeavour(controller, tagMap);
	}

	public bool usesTagType(TagRequirement type) {
		foreach (TagRequirement tagType in getRequiredTagsList()) {
			if (tagType.type == type.type && type.stale == tagType.stale) {
				return true;
			}
		}
		return false;
	}

	protected abstract Endeavour createEndeavour (RobotController controller, Dictionary<TagEnum, Tag> tagMap);

    public static List<TagRequirement> getRequiredTags() {
		return null;
	}

	public List<TagRequirement> getRequiredTagsList() {
		if (requiredTagsCache == null) {
			requiredTagsCache = (List<TagRequirement>)GetType().GetMethod("getRequiredTags").Invoke(null, null);
		}
		return requiredTagsCache;
	}

    public static EndeavourFactory constructDefault() {
		EndeavourFactory factory = (EndeavourFactory) types[0].GetConstructor(new System.Type[0]).Invoke(new object[0]);
		factory.goals = new List<Goal> ();
		return factory;
	}

	private static string[] getTypeNames() {
		if (typeNames == null || typeNames.Length != types.Length) {
			typeNames = new string[types.Length];
			for(int i=0; i<typeNames.Length; ++i) {
				typeNames[i] = types[i].FullName;
			}
		}
		return typeNames;
	}

#if UNITY_EDITOR
        InspectorListElement InspectorListElement.doListElementGUI(GameObject parent) {
		int selectedType = System.Array.FindIndex(types, OP => OP == GetType());
		int newSelectedType = UnityEditor.EditorGUILayout.Popup(selectedType, getTypeNames());
		if (newSelectedType != selectedType) {
			return (EndeavourFactory)EndeavourFactory.types[newSelectedType].GetConstructor(new System.Type[0]).Invoke(new object[0]);
		}

		doGUI();
		return this;
	}

	public virtual void doGUI() {
		optimalMobSize = Mathf.Max(1, UnityEditor.EditorGUILayout.IntField("Optimal Mob Size", optimalMobSize));
		maxMobBenefit = UnityEditor.EditorGUILayout.FloatField("Max Mob Benefit", maxMobBenefit);
		mobCostPerRobot = UnityEditor.EditorGUILayout.FloatField("Cost per Mob Robot", mobCostPerRobot);
		UnityEditor.EditorGUILayout.Separator();

		status = UnityEditor.EditorGUILayout.Foldout(status, "Goals");

		if (status && goals != null) {
			size = UnityEditor.EditorGUILayout.IntField("Size", goals.Count);
			UnityEditor.EditorGUILayout.Separator();

			foreach (Goal goal in goals) {
				//goal.name = EditorGUILayout.TextField("Name", goal.name);
				//int selectedType = (int) goal.type; // System.Array.FindIndex(goalEnums, OP => OP == goal.type);
				goal.type = (GoalEnum) UnityEditor.EditorGUILayout.Popup((int) goal.type, System.Enum.GetNames(typeof(GoalEnum)));
				goal.priority = UnityEditor.EditorGUILayout.FloatField("Priority", goal.priority);
				UnityEditor.EditorGUILayout.Separator();
			}
			if (size < goals.Count) {
				goals.RemoveRange(size, goals.Count - size);
			}
			while (size > goals.Count) {
				goals.Add(new Goal((GoalEnum)System.Enum.GetValues(typeof(GoalEnum)).GetValue(0), 0));
			}
		}
	}
#endif
}