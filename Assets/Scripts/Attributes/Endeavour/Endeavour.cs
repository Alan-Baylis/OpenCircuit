using UnityEngine;
using System.Collections;
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

    public bool active = false;

    private float priorityCache;
    private int lastFrameEvaluated = -1;
	private Dictionary<TagEnum, Tag> tagMap;

	public Endeavour(EndeavourFactory parentFactory, RobotController controller, List<Goal> goals, Dictionary<TagEnum, Tag> tagMap) {
		this.controller = controller;
		this.goals = goals;
		this.factory = parentFactory;
		this.tagMap = tagMap;
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
	protected virtual void onStopExecution() {}

	public virtual void onMessage(RobotMessage message) {}

	public bool isReady(Dictionary<System.Type, int> availableComponents) {
		return (!singleExecutor() || tagMap[getPrimaryTagType()].getConcurrentExecutions(controller, GetType()) == 0) && canExecute() && hasAllComponents(availableComponents);
	}

	private bool hasAllComponents(Dictionary<System.Type, int> availableComponents) {
		foreach(System.Type type in getRequiredComponents()) {
			if(!availableComponents.ContainsKey(type) || availableComponents[type] < 1) {
				return false;
			}
		}

		foreach(System.Type type in getRequiredComponents()) {
			int numAvailable = availableComponents[type];
			if(numAvailable > 0) {
				--numAvailable;
				availableComponents[type] = numAvailable;
			}
		}
		return true;
	}

	public abstract System.Type[] getRequiredComponents();

	public abstract bool canExecute();

	public abstract bool singleExecutor();

	public abstract TagEnum getPrimaryTagType();

	public float getPriority() {
        if (lastFrameEvaluated != Time.frameCount) {
            priorityCache = calculateFinalPriority();
            lastFrameEvaluated = Time.frameCount;
        }
        return priorityCache;
	}
	
	protected float calculateFinalPriority() {
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

#if UNITY_EDITOR
	public string getTargetName() {
		return getTagOfType<Tag>(getPrimaryTagType()).getLabelHandle().getName();
	}
#endif

	public RobotController getController() {
		return controller;
	}
	
	public bool Equals(Endeavour endeavour) {
		return controller == endeavour.controller && name.Equals(endeavour.name);
	}

    protected abstract float getCost();

    protected virtual float calculatePriority() {
		float calculatedPriority = 0;
		foreach (Goal goal in goals) {
			Dictionary<GoalEnum, Goal> robotGoals = controller.getGoals();
			if (robotGoals.ContainsKey(goal.type)) {
				float priorityCubed = (goal.priority * goal.priority * goal.priority);
				calculatedPriority += priorityCubed * robotGoals[goal.type].priority;
			}
		}
		return calculatedPriority + BENEFIT_CONSTANT_TERM;
	}

	protected float calculateMobBenefit() {
		int executors = tagMap[getPrimaryTagType()].getConcurrentExecutions(controller, GetType());
		return Mathf.Min(factory.maxMobBenefit, factory.maxMobBenefit *(executors + 1f) / factory.optimalMobSize)
			   -executors * factory.mobCostPerRobot;
	}

	protected T getTagOfType<T>(TagEnum tagType) where T : Tag {
		//Debug.Log("getting tag of type: " + tagType.ToString());
		return (T)tagMap[tagType];
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
}
