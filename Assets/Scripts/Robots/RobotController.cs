using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[AddComponentMenu("Scripts/Robot/Robot Controller")]
public class RobotController : NetworkBehaviour, ISerializationCallbackReceiver, MentalModelUpdateListener {

	[SerializeField]
	public byte[] serializedData;

	private HashSet<Endeavour> availableEndeavours = new HashSet<Endeavour> (new EndeavourComparer());
	private Dictionary<Tag, List<Endeavour>> tagUsageMap = new Dictionary<Tag, List<Endeavour>>();
	private AudioSource soundEmitter;
	private Health myHealth;
    private Timing executionTimer;

	public Health health { get { return myHealth; } }

	public EffectSpec destructionEffect;

#if UNITY_EDITOR
	public bool debug = false;
	public bool shouldAlphabetize = false;
	//Used for debugging sound
	//[System.NonSerialized]
	//public List<GameObject> trackedObjects = new List<GameObject>();
#endif

	public Label[] locations;
    public Goal[] goals;
	[System.NonSerialized]
	public EndeavourFactory[] endeavourFactories = new EndeavourFactory[0];
	[System.NonSerialized]
	public Dictionary<GoalEnum, Goal> goalMap = new Dictionary<GoalEnum, Goal>();

    public float reliability = 5f;
	public AudioClip targetSightedSound;
    public float evaluatePeriod = .2f;

	[System.NonSerialized]
	private HashSet<Endeavour> currentEndeavours = new HashSet<Endeavour>();
	[System.NonSerialized]
	private Dictionary<System.Type, List<AbstractRobotComponent>> myComponentMap = null;

	[System.NonSerialized]
	MentalModel mentalModel = new MentalModel ();
	[System.NonSerialized]
	MentalModel externalMentalModel = null;

	[ServerCallback]
	void Start() {
        mentalModel.addUpdateListener(this);
		myHealth = GetComponent<Health>();
		soundEmitter = gameObject.AddComponent<AudioSource>();
	    foreach(Goal goal in goals) {
            if(!goalMap.ContainsKey(goal.type)) {
                goalMap.Add(goal.type, goal);
            }
        }

	    Label[] labels = FindObjectsOfType<Label>();
	    foreach (Label label in labels) {
            if (label.inherentKnowledge) {
                sightingFound(label.labelHandle, label.transform.position, null);
            }
        }

        foreach (Label location in locations) {
			if (location == null) {
				Debug.LogWarning("Null location attached to AI with name: " + gameObject.name);
				continue;
			}
			sightingFound(location.labelHandle, location.transform.position, null);
		}
		InvokeRepeating ("evaluateActions", .1f, evaluatePeriod);
		constructAllEndeavours();
	}

    public Dictionary<System.Type, List<AbstractRobotComponent>> componentMap {
        get {
            if (myComponentMap == null) {
                AbstractRobotComponent[] components = GetComponentsInChildren<AbstractRobotComponent>();
                myComponentMap = new Dictionary<System.Type, List<AbstractRobotComponent>>();
                foreach (AbstractRobotComponent component in components) {
                    component.attachToController(this);
                }
            }
            return myComponentMap;
        }
    }

	// Update is called once per frame
	[ServerCallback]
	void Update () {
//Leave this here, very useful!!
//#if UNITY_EDITOR

//		if(debug) {
//			/* 
//			* Draw tracked targets
//			*/
//			foreach(GameObject obj in trackedObjects) {
//				Destroy(obj);
//			}
//			trackedObjects.Clear();
//			foreach(LabelHandle handle in trackedTargets) {
//				GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
//				capsule.transform.position = handle.getPosition();
//				capsule.GetComponent<MeshRenderer>().material.color = Color.yellow;
//				capsule.transform.localScale = new Vector3(.2f, .2f, .2f);
//				Destroy(capsule.GetComponent<Rigidbody>());
//				Destroy(capsule.GetComponent<CapsuleCollider>());
//				trackedObjects.Add(capsule);
//			}
//		}
//#endif
//	    double startTime = Time.realtimeSinceStartup;
		List<Endeavour> endeavours = new List<Endeavour>(currentEndeavours);
		foreach(Endeavour endeavour in endeavours) {
			try {
				endeavour.update();

			} catch (Exception e) {
				Debug.LogError("Endeavour '" + endeavour.getName() + "' threw an exception during update.");
				Debug.LogException(e);
			}
		}
//	    double endTime = Time.realtimeSinceStartup;
//	    executionTimer.addTime(endTime-startTime);
	}

	public bool knowsTarget(LabelHandle target) {
		return getMentalModel ().canSee (target);
	}

	public Vector3? getLastKnownPosition(LabelHandle target) {
		return getMentalModel().getLastKnownPosition(target);
	}

	public void attachMentalModel(MentalModel model) {
		externalMentalModel = model;
        externalMentalModel.addUpdateListener(this);
	}

	public void enqueueMessage(RobotMessage message) {
		List<Endeavour> endeavours = new List<Endeavour>(currentEndeavours);
		foreach (Endeavour action in endeavours) {
			action.onMessage(message);
		}
	}

	public Dictionary<GoalEnum, Goal> getGoals() {
		return goalMap;
	}

    public void attachRobotComponent(AbstractRobotComponent component) {
        if (!componentMap.ContainsKey(component.getComponentArchetype())) {
            componentMap[component.getComponentArchetype()] = new List<AbstractRobotComponent>();
        }
        componentMap[component.getComponentArchetype()].Add(component);
    }

    public void detachRobotComponent(AbstractRobotComponent component) {
        componentMap.Remove(component.getComponentArchetype());
    }

    public T getRobotComponent<T> () where T : AbstractRobotComponent {
		return (T)getRobotComponent(typeof(T));
	}

	public AbstractRobotComponent getRobotComponent(System.Type type) {
		List<AbstractRobotComponent> compList;
		componentMap.TryGetValue(type, out compList);
		return compList == null ? null : compList[0];
	}

    public void addTag(Tag newTag) {
		foreach (EndeavourFactory factory in endeavourFactories) {
			if (factory.usesTagType(new TagRequirement(newTag.type, !getMentalModel().canSee(newTag.getLabelHandle())))) {
				List<List<Tag>> tagSets = new List<List<Tag>>();
				tagSets.Add(new List<Tag> { newTag });

				List<TagRequirement> requiredTags = factory.getRequiredTagsList();
				foreach(TagRequirement tagType in requiredTags) {
					if (tagType.type != newTag.type) {
						tagSets.Add(getMentalModel().getTagsOfType(tagType.type, tagType.stale));
					}
				}

				if (tagSets.Count > 0) {
					List<Tag> chosen = new List<Tag>();
					List<Endeavour> endeavours = constructWithCombination(factory, tagSets, chosen, 0);
					foreach (Endeavour endeavour in endeavours) {
						foreach (Tag tag in endeavour.getTagsInUse()) {
							addTagUsageEntry(tag, endeavour);
						}
						availableEndeavours.Add(endeavour);
					}
				}
			}
		}
    }

    public void removeTag(Tag tag) {
		// No entry for the tag simply means that no endeavours are using that tag
		if (tagUsageMap.ContainsKey(tag)) {
			List<Endeavour> endeavours = tagUsageMap[tag];
			while (endeavours.Count > 0) {
				removeEndeavour(endeavours[endeavours.Count - 1]);
			}
		}
    }

    public void attachExecutionTimer(Timing timer) {
        executionTimer = timer;
    }

    public Timing getExecutionTimer() {
        return executionTimer;
    }

    private void constructAllEndeavours() {
        foreach (EndeavourFactory factory in endeavourFactories) {
			List<List<Tag>> tagSets = new List<List<Tag>>();
            List<TagRequirement> requiredTags = factory.getRequiredTagsList();
            foreach (TagRequirement tagType in requiredTags) {
                tagSets.Add(getMentalModel().getTagsOfType(tagType.type, tagType.stale));
            }

			if (tagSets.Count > 0) {
				List<Tag> chosen = new List<Tag>();
				List<Endeavour> endeavours = constructWithCombination(factory, tagSets, chosen, 0);
				foreach (Endeavour endeavour in endeavours) {
					foreach (Tag tag in endeavour.getTagsInUse()) {
						addTagUsageEntry(tag, endeavour);
					}
					availableEndeavours.Add(endeavour);
				}
			}
        }
	}

    private List<Endeavour> constructWithCombination(EndeavourFactory factory, List<List<Tag>> tagset, List<Tag> chosen, int index) {
		List<Endeavour> results = new List<Endeavour>();
        foreach (Tag tag in tagset[index]) {
			chosen.Add(tag); // We have chosen this tag
			//base case - do not recurse
			if (index == tagset.Count - 1) {
				results.Add(factory.constructEndeavour(this, new List<Tag>(chosen)));
			} else {
				results.AddRange(constructWithCombination(factory, tagset, chosen, ++index));
			}
			chosen.RemoveAt(chosen.Count - 1); // Remove it from the end to try the next one
		}
		return results;
	}

	private void addTagUsageEntry(Tag tag, Endeavour endeavour) {
		if (tagUsageMap.ContainsKey(tag)) {
			tagUsageMap[tag].Add(endeavour);
		} else {
			List<Endeavour> endeavours = new List<Endeavour>();
			endeavours.Add(endeavour);
			tagUsageMap.Add(tag, endeavours);
		}
	}

	private void evaluateActions() {
#if UNITY_EDITOR
		List<DecisionInfoObject> debugText = new List<DecisionInfoObject>();
#endif
		//print("****EVALUATE****");
        DictionaryHeap endeavourQueue = new DictionaryHeap();
		List<Endeavour> staleEndeavours = new List<Endeavour>();
		//print("\tCurrent Endeavours");
		foreach(Endeavour action in currentEndeavours) {
			if(action.isStale()) {
				//print("\t\tstale: " + action.getName());
				action.stopExecution();
				staleEndeavours.Add(action);
			} else {
				//print("\t\t++" + action.getName());
				availableEndeavours.Add(action);
			}
		}
	    Dictionary<System.Type, int> componentMap = getComponentUsageMap();

		//print("\tAvailable Endeavours");
		foreach(Endeavour action in availableEndeavours) {
			try {
				if (action.isStale()) {
					//print("\t\t--" + action.getName());
					staleEndeavours.Add(action);
				} else if (!action.isMissingComponents(componentMap)) {
					//print("\t\t++" + action.getName());
					endeavourQueue.Enqueue(action);
				}
			} catch (Exception e) {
				Debug.LogError("Endeavour '" +action.getName()+"' threw an exception from isStale()");
				Debug.LogException(e);
			}
		}

		foreach(Endeavour action in staleEndeavours) {
			availableEndeavours.Remove(action);
			currentEndeavours.Remove(action);
		}
		HashSet<Endeavour> proposedEndeavours = new HashSet<Endeavour>();


#if UNITY_EDITOR
		bool maxPrioritySet = false;
		float localMaxPriority = 0;
		float localMinPriority = 0;
#endif

		while(endeavourQueue.Count > 0) {
			Endeavour action = (Endeavour)endeavourQueue.Dequeue();
			bool isReady = action.isReady(componentMap);
#if UNITY_EDITOR
			if (debug && !action.isMissingComponents(componentMap)) {
				float priority = action.getPriority();
				if (!maxPrioritySet) {
					maxPrioritySet = true;
					localMaxPriority = priority;
					localMinPriority = priority;
				} else {
					if (priority > localMaxPriority) {
						localMaxPriority = priority;
					}
					if (priority < localMinPriority) {
						localMinPriority = priority;
					}
				}
				debugText.Add(new DecisionInfoObject(action.getName(), action.getTargetName(), priority, isReady));
			}
#endif
			if (isReady) {
				if(proposedEndeavours.Contains(action)) {
					Debug.LogError("action already proposed!!!");
				}
				proposedEndeavours.Add(action);
				availableEndeavours.Remove(action);
			} else {
				if(action.active) {
					action.stopExecution();
				}
			}
		}

		// All previous actions MUST be stopped before we start the new ones
		foreach(Endeavour action in proposedEndeavours) {
			if(!action.active) {
				action.execute();
			}
		}
	
		currentEndeavours = proposedEndeavours;
#if UNITY_EDITOR
		if(debug) {
			lines = debugText;
			maxPriority = Mathf.Abs(localMaxPriority) > Mathf.Abs(localMinPriority) ? Mathf.Abs(localMaxPriority) : Mathf.Abs(localMinPriority);
			if(shouldAlphabetize) {
				alphabetize();
			}
		}
#endif
	}

	private void removeEndeavour(Endeavour endeavour) {
		if (availableEndeavours.Contains(endeavour)) {
			availableEndeavours.Remove(endeavour);
		} else if (currentEndeavours.Contains(endeavour)) {
			endeavour.stopExecution();
			currentEndeavours.Remove(endeavour);
		}
		foreach(Tag tag in endeavour.getTagsInUse()) {
			if (tagUsageMap.ContainsKey(tag)) {
				tagUsageMap[tag].Remove(endeavour);
			}
		}
	}

	private Dictionary<System.Type, int> getComponentUsageMap() {
		Dictionary<System.Type, int> componentUsageMap = new Dictionary<System.Type, int>();
        foreach (List<AbstractRobotComponent> componentList in componentMap.Values) {
            foreach (AbstractRobotComponent component in componentList) {
                if (componentUsageMap.ContainsKey(component.getComponentArchetype())) {
                    int count = componentUsageMap[component.getComponentArchetype()];
                    ++count;
                    componentUsageMap[component.getComponentArchetype()] = count;
                } else {
                    componentUsageMap[component.getComponentArchetype()] = 1;
                }
            }
        }
		return componentUsageMap;
	}

	public void sightingLost(LabelHandle target, Vector3 lastKnownPos, Vector3? lastKnownVelocity) {
		if (externalMentalModel != null) {
			externalMentalModel.removeSighting(target, lastKnownPos, lastKnownVelocity);
		}
		mentalModel.removeSighting(target, lastKnownPos, lastKnownVelocity);
	}

	public void sightingFound(LabelHandle target, Vector3 pos, Vector3? dir) {
		if (externalMentalModel != null) {
			externalMentalModel.addSighting(target, pos, dir);
		}
		mentalModel.addSighting(target, pos, dir);
	}

	public MentalModel getMentalModel() {
		if (externalMentalModel == null) {
			return mentalModel;
		}
		return externalMentalModel;
	}

	[Server]
	public void dispose() {
		GlobalConfig.globalConfig.subtractRobotCount(this);
		CancelInvoke();
		foreach(Endeavour e in currentEndeavours) {
			e.stopExecution();
		}
		enabled = false;

		int randomSeed = UnityEngine.Random.seed;

        disassembleRobotComponents();
		RpcDismantle(randomSeed);
	}

	[ClientRpc]
	protected void RpcDismantle(int randomSeed) {
		dismantle();
		destructionEffect.spawn(transform.position);
	}

    [Server]
    protected void disassembleRobotComponents() {
        foreach (List<AbstractRobotComponent> componentList in componentMap.Values) {
            foreach (AbstractRobotComponent component in componentList) {
                component.enabled = false;
                component.transform.parent = null;
                component.dismantle();
            }
        }
    }

	protected void dismantle() {
        int breaker = 0;
        for (int i = transform.childCount-1; i >= 0 && breaker < 100; --i) {
            breaker++;
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
        Destroy(gameObject);
	}

#if UNITY_EDITOR
	public List<DecisionInfoObject> lines = new List<DecisionInfoObject>();
	public float maxPriority = 0;
	public float minPriority = 0;

	void alphabetize() {
		if(lines.Count > 0) {
			List<DecisionInfoObject> newList = new List<DecisionInfoObject>();
			newList.Add(lines[0]);
			lines.RemoveAt(0);
			foreach(DecisionInfoObject obj in lines) {
				for(int i = 0; i < newList.Count; ++i) {
					if(obj.getTitle().CompareTo(newList[i].getTitle()) < 0) {
						newList.Insert(i, obj);
						break;
					} else if(i == newList.Count - 1) {
						newList.Add(obj);
						break;
					}
				}
			}
			lines = newList;
		}
	}

	void OnGUI() {
		if(debug) {
			Camera cam = Camera.current;
			Vector3 pos;
			if(cam != null) {
				Vector3 worldTextPos = transform.position + new Vector3(0, 1, 0);
				pos = cam.WorldToScreenPoint(worldTextPos);
				if(Vector3.Dot(cam.transform.forward, (worldTextPos - cam.transform.position).normalized) < 0) {
					return;
				}
			} else {
				return;
			}
			
			int shownLines = Mathf.Min(8, lines.Count);

			GUI.enabled = true;
			string buffer = "";
			for(int i = 0; i < shownLines; i++) {
				buffer += "\n";
			}


			Texture2D red = new Texture2D(1, 1);
			Color transparentRed = new Color(0f, .0f, .0f, .4f);

			red.SetPixel(0, 0, transparentRed);
			red.Apply();

			Texture2D blue = new Texture2D(1, 1);
			Color transparentBlue = new Color(.1f, .1f, 1f, .6f);
			blue.SetPixel(0, 0, transparentBlue);
			blue.alphaIsTransparency = true;

			blue.Apply();

			Texture2D green = new Texture2D(1, 1);
			Color transparentGreen = new Color(.1f, 1f, .1f, .4f);
			green.SetPixel(0, 0, transparentGreen);
			green.alphaIsTransparency = true;

			green.Apply();

			float lineHeight = 30f;
			Vector2 size = new Vector2(200, shownLines * lineHeight);
			for (int i = 0; i < shownLines; ++i) {
				DecisionInfoObject obj = lines[i];
				float percentFilled = Mathf.Abs(obj.getPriority()) / maxPriority;
				if(obj.getPriority() < 0) {
					percentFilled = -percentFilled;
				}

				Rect rectng = new Rect(pos.x - size.x / 2, Screen.height - pos.y - size.y + i * lineHeight, size.x, lineHeight);

				GUI.skin.box.normal.background = red;

				GUI.Box(rectng, GUIContent.none);

				Rect filled;
				if(percentFilled < 0) {
					filled = new Rect(pos.x + size.x / 2 * percentFilled, Screen.height - pos.y - size.y + i * lineHeight, -(size.x / 2 * percentFilled), lineHeight);
				} else {
					filled = new Rect(pos.x, Screen.height - pos.y - size.y + i * lineHeight, size.x / 2 * percentFilled, lineHeight);
				}

				//GUI.skin.box.normal.background = green;
				//GUI.Box(filled, GUIContent.none);
				GUI.DrawTexture(filled, blue);
				//if(obj.isChosen()) {
				//	GUI.Label(textCentered, "+" + obj.getTitle());
				//} else {
				//}
				Rect boxRectangle = new Rect(pos.x - size.x/2, Screen.height - pos.y - size.y + i*lineHeight+lineHeight/4, 15, 15);
				if(obj.isChosen()) {
					GUI.DrawTexture(boxRectangle, green);
				} else {
					GUI.DrawTexture(boxRectangle, red);
				}

				Rect textCentered = new Rect(pos.x - size.x / 2 + 17, Screen.height - pos.y - size.y + i * lineHeight + 4, size.x, lineHeight);
				GUI.Label(textCentered, obj.getTitle());


				Font font = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/GUI/Courier.ttf");
				GUIStyle style = new GUIStyle(GUI.skin.label);
				style.font = font;
				style.fontSize = 14;
				
				string priorityString = obj.getPriority().ToString("0.#0").PadLeft(7);
				Vector2 labelSize = style.CalcSize(new GUIContent(priorityString));
				//Rect center = new Rect(pos.x - labelSize.x/2 - 7, Screen.height - pos.y - size.y + (i * lineHeight), labelSize.x, labelSize.y);
				Rect center = new Rect(pos.x + size.x/2 - labelSize.x, Screen.height - pos.y - size.y + (i * lineHeight), labelSize.x, labelSize.y);
				GUI.Label(center, priorityString, style);

				string sourceString = obj.getSource();
				Vector2 sourceStringSize = style.CalcSize(new GUIContent(sourceString));
				Rect sourceRect = new Rect(pos.x + size.x / 2 - sourceStringSize.x, Screen.height - pos.y - size.y + (i * lineHeight) + lineHeight/2, sourceStringSize.x, sourceStringSize.y);
				GUI.Label(sourceRect, sourceString, style);
			}
			/* 
			 * Draw the battery meter
			*/
			Battery battery = GetComponentInChildren<Battery>();
			if(battery != null) {
				Rect progressBar = new Rect(pos.x - size.x / 2, Screen.height - pos.y + 3, size.x, 20);

				GUI.skin.box.normal.background = red;
				GUI.Box(progressBar, GUIContent.none);

				GUI.skin.box.normal.background = green;

				Rect progressBarFull = new Rect(pos.x - size.x / 2, Screen.height - pos.y + 3, size.x * (battery.currentCapacity / battery.maximumCapacity), 20);
				GUI.Box(progressBarFull, GUIContent.none);
			}
		}
	}
#endif

	public void OnBeforeSerialize() {
		lock(this) {
			MemoryStream stream = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, endeavourFactories);

			serializedData = stream.ToArray();
			stream.Close();
		}
	}

	public void OnAfterDeserialize() {
		lock(this) {
			MemoryStream stream = new MemoryStream(serializedData);
			BinaryFormatter formatter = new BinaryFormatter();
			endeavourFactories = (EndeavourFactory[])formatter.Deserialize(stream);

			foreach(EndeavourFactory factory in endeavourFactories) {
				if(factory != null) {
					if(factory.goals == null) {
						factory.goals = new List<Goal>();
					}
				}
			}
			stream.Close();
		}
	}
}