﻿using UnityEngine;
using System.Collections.Generic;

/**
 * Something an AI can endeavour to accomplish, or more simply put, execute
 */
[System.Serializable]
public abstract class Endeavour : Prioritizable {

	public const float BENEFIT_CONSTANT_TERM = 20f;

    public float momentum = 1f;

	public List<Goal> goals = new List<Goal>();
	
	protected string name;

	protected RobotController controller;
	protected EndeavourFactory factory;

    public bool active;

	protected float priorityCache;
    protected int lastFrameEvaluated = -1;
	protected int firstFrame = -1;
	protected Dictionary<TagEnum, Tag> tagMap;

	private const int EVALUATION_DELAY_FRAMES = 2;
	
    private AbstractArms myArms;
	private AbstractRobotGun myRifle;
	private HoverJet myJet;
	private RoboEyes myEyes;
	private TowerSpawner myTowerSpawner;


	public Endeavour(EndeavourFactory parentFactory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tagMap) {
		this.controller = controller;
		this.goals = goals;
		this.tagMap = tagMap;
		factory = parentFactory;
		firstFrame = Time.frameCount;
		lastFrameEvaluated = Time.frameCount;
	}

	public virtual void update() {

	}

	public abstract bool isStale();

	public void execute() {
		active = true;
		recordExecution();
		onExecute();
	}

	protected abstract void onExecute();

    public void stopExecution() {
        active = false;
		withdrawExecution();

		foreach(System.Type component in getRequiredComponents()) {
			AbstractRobotComponent roboComp = getController().getRobotComponent(component);
			if (roboComp != null) {
				roboComp.release();
			}
		}

		onStopExecution();
    }


	public virtual bool isReady(Dictionary<System.Type, int> availableComponents) {
		return (!singleExecutor() || tagMap[getPrimaryTagType()].getConcurrentExecutions(controller, GetType()) == 0) && canExecute() && hasAllComponents(availableComponents);
	}

	public virtual float getPriority() {
		int currentFrame = Time.frameCount;
		if (lastFrameEvaluated != currentFrame && Time.frameCount - firstFrame % EVALUATION_DELAY_FRAMES == 0) {
			priorityCache = calculateFinalPriority();
			lastFrameEvaluated = Time.frameCount;
		}
		return priorityCache;
	}

	public List<TagRequirement> getRequiredTags() {
		return factory.getRequiredTagsList();
	}

	public List<Tag> getTagsInUse() {
		return new List<Tag>(tagMap.Values);
	}

	protected virtual float calculateFinalPriority() {
		float finalPriority = calculatePriority();
		finalPriority += calculateMobBenefit();
		if (active) {
			finalPriority += controller.reliability * momentum;
		}
		finalPriority -= getCost();
		return finalPriority;
	}

	public string getName() {
		return name;
	}

	public RobotController getController() {
		return controller;
	}

	public bool Equals(Endeavour endeavour) {
		return controller == endeavour.controller && name.Equals(endeavour.name);
	}

	public T getTagOfType<T>(TagEnum tagType) where T : Tag {
		//Debug.Log("getting tag of type: " + tagType.ToString());
		return (T)tagMap[tagType];
	}

	public virtual void onMessage(RobotMessage message) { }

	public abstract System.Type[] getRequiredComponents();

	public abstract bool canExecute();

	public abstract bool singleExecutor();

	public abstract TagEnum getPrimaryTagType();

	protected abstract float getCost();

	protected virtual void onStopExecution() { }


#if UNITY_EDITOR
	public string getTargetName() {
		return getTagOfType<Tag>(getPrimaryTagType()).getLabelHandle().getName();
	}
#endif

    protected virtual float calculatePriority() {
		float calculatedPriority = 0;
		foreach (Goal goal in goals) {
			Goal[] robotGoals = controller.getGoals();
			if (robotGoals[(int)goal.type] != null) {
				float priorityCubed = (goal.priority * goal.priority * goal.priority);
				calculatedPriority += priorityCubed * robotGoals[(int)goal.type].priority;
			}
		}
		return calculatedPriority + BENEFIT_CONSTANT_TERM;
	}

	protected float calculateMobBenefit() {
		int executors = tagMap[getPrimaryTagType()].getConcurrentExecutions(controller, GetType());
		return Mathf.Min(factory.maxMobBenefit, factory.maxMobBenefit *(executors + 1f) / factory.optimalMobSize)
			   -executors * factory.mobCostPerRobot;
	}

    protected AbstractArms arms {
        get {
            if (myArms == null) {
                myArms = getController().getRobotComponent<AbstractArms>();
            }
            return myArms;
        }
    }

	protected AbstractRobotGun rifle {
		get {
			if (myRifle == null) {
				myRifle = getController().getRobotComponent<AbstractRobotGun>();
			}
			return myRifle;
		}
	}

    protected HoverJet jet {
        get {
            if (myJet == null) {
                myJet = getController().getRobotComponent<HoverJet>();
            }
            return myJet;
        }
    }

	protected RoboEyes eyes {
		get {
			if (myEyes == null) {
				myEyes = getController().getRobotComponent<RoboEyes>();
			}
			return myEyes;
		}
	}

    protected TowerSpawner towerSpawner {
        get {
            if (myTowerSpawner == null) {
                myTowerSpawner = getController().getRobotComponent<TowerSpawner>();
            }
            return myTowerSpawner;
        }
    }

	protected bool hasAllComponents(Dictionary<System.Type, int> availableComponents) {
		foreach (System.Type type in getRequiredComponents()) {
			if (!availableComponents.ContainsKey(type) || availableComponents[type] < 1) {
				return false;
			}
		}

		foreach (System.Type type in getRequiredComponents()) {
			int numAvailable = availableComponents[type];
			if (numAvailable > 0) {
				--numAvailable;
				availableComponents[type] = numAvailable;
			}
		}
		return true;
	}

	private void withdrawExecution() {
		foreach (Tag tag in tagMap.Values) {
			tag.removeExecution(getController(), this.GetType());
		}
	}

	private void recordExecution() {
		foreach (Tag tag in tagMap.Values) {
			tag.addExecution(getController(), this.GetType());
		}
	}

    public bool isMissingComponents(Dictionary<System.Type, int> availableComponents) {
        foreach (System.Type type in getRequiredComponents()) {
            if (!availableComponents.ContainsKey(type)) {
                return true;
            }
        }
        return false;
    }
}
