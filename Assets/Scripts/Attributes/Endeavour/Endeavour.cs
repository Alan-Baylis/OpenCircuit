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

	protected System.Type[] requiredComponents;

	protected LabelHandle parent;

	protected EndeavourFactory factory;

    public bool active = false;

	public Endeavour(EndeavourFactory parentFactory, RobotController controller, List<Goal> goals, LabelHandle parent) {
		this.controller = controller;
		this.goals = goals;
		this.parent = parent;
		this.factory = parentFactory;
	}

	public abstract bool isStale();

	public virtual void execute () {
        active = true;
		factory.addExecution();
    }

    public virtual void stopExecution() {
        active = false;
		factory.removeExecution();
    }
	public abstract void onMessage(RobotMessage message);

	public bool isReady(Dictionary<System.Type, int> availableComponents) {
		return (!singleExecutor() || factory.getConcurrentExecutions() == 0) && canExecute() && hasAllComponents(availableComponents);
	}

	private bool hasAllComponents(Dictionary<System.Type, int> availableComponents) {
		foreach(System.Type type in requiredComponents) {
			if(!availableComponents.ContainsKey(type) || availableComponents[type] < 1) {
				return false;
			}
		}

		foreach(System.Type type in requiredComponents) {
			int numAvailable = availableComponents[type];
			if(numAvailable > 0) {
				--numAvailable;
				availableComponents[type] = numAvailable;
			}
		}
		return true;
	}

	public abstract bool canExecute();

	public abstract bool singleExecutor();

	public virtual float getPriority() {
		float finalPriority = 0;
		foreach (Goal goal in goals) {
			Dictionary<GoalEnum, Goal> robotGoals = controller.getGoals ();
			if (robotGoals.ContainsKey(goal.type)) {
				float priorityCubed = (goal.priority * goal.priority * goal.priority) ;
				finalPriority += priorityCubed * robotGoals[goal.type].priority;
			}
		}
		float cost = getCost ();
        if (active) {
            finalPriority += controller.reliability * momentum;
        }
		return finalPriority - cost + BENEFIT_CONSTANT_TERM;
	}

	protected abstract float getCost ();
	
	public string getName() {
		return name;
	}
	
	public RobotController getController() {
		return controller;
	}

	public LabelHandle getParent() {
		return parent;
	}
	
	public bool Equals(Endeavour endeavour) {
		return controller == endeavour.controller && name.Equals(endeavour.name);
	}
}
