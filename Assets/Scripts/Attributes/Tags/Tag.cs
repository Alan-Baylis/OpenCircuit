﻿using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public class Tag : InspectorListElement {

	public static Tag constructDefault() {
		return new Tag((TagEnum)Enum.GetValues(typeof(TagEnum)).GetValue(0), 0, null);
	}

	public static Tag constructTag(TagEnum type) {
		switch (type) {
			case TagEnum.PatrolRoute:
				return new PatrolTag(0, null);
			case TagEnum.AttackRoute:
				return new AttackRoute(0, null);
			case TagEnum.BuildDirective:
				return new BuildDirectiveTag(null, 0, null);
			case TagEnum.GuardPoint:
				return new GuardTag(0, null);
			default:
				return new Tag(type, 0, null);
		}
	}

	public static readonly TagEnum[] tagEnums;

	public TagEnum type;
	public float severity;

	[System.NonSerialized]
	protected Dictionary<System.Type, HashSet<RobotController>> executors = new Dictionary<System.Type, HashSet<RobotController>>();

	[System.NonSerialized]
	protected Dictionary<System.Type, Bidder> myBidders = new Dictionary<Type, Bidder>();

	protected Dictionary<System.Type, Bidder> bidders {
		get {
			if (myBidders == null)
				myBidders = new Dictionary<Type, Bidder>();
			return myBidders;
		}
	}

	[System.NonSerialized]
    private LabelHandle labelHandle;

	public Tag(TagEnum type, float severity, LabelHandle handle) {
		this.type = type;
		this.severity = severity;
		labelHandle = handle;
	}

    public void setLabelHandle(LabelHandle label) {
        labelHandle = label;
    }

    public LabelHandle getLabelHandle() {
        return labelHandle;
    }

	public bool makeBid(RobotController robotController, System.Type endeavourType, float amount) {
		if (bidders.ContainsKey(endeavourType)) {
			if (amount > bidders[endeavourType].value || bidders[endeavourType].bidder == robotController) {
				bidders[endeavourType] = new Bidder(robotController, amount);
				return true;
			}
			return false;
		}
		bidders[endeavourType] = new Bidder(robotController, amount);
		return true;
	}

	public void withdrawBid(RobotController robotController, System.Type endeavourType) {
		if (bidders.ContainsKey(endeavourType)) {
			if (bidders[endeavourType].bidder == robotController) {
				bidders.Remove(endeavourType);
			}
		}
	}

	public bool hasBid(RobotController robotController, System.Type endeavourType) {
		return bidders.ContainsKey(endeavourType) && bidders[endeavourType].bidder == robotController;
	}

	public void addExecution(RobotController executor, System.Type endeavourType) {
		getExecutors(endeavourType).Add(executor);
	}

	public void removeExecution(RobotController executor, System.Type endeavourType) {
		getExecutors(endeavourType).Remove(executor);
	}

	public int getConcurrentExecutions(RobotController executor, System.Type endeavourType) {
		HashSet<RobotController> endeavourExecutors = getExecutors(endeavourType);
		if (endeavourExecutors.Contains(executor)) {
			return endeavourExecutors.Count - 1;
		}
		return endeavourExecutors.Count;
	}

	private HashSet<RobotController> getExecutors(System.Type endeavourType) {
		if (executors == null) {
			executors = new Dictionary<Type, HashSet<RobotController>>();
		}
		if (!executors.ContainsKey(endeavourType)) {
			executors[endeavourType] = new HashSet<RobotController>();
		}
		return executors[endeavourType];
	}

	protected class Bidder {
		private RobotController myBidder;
		private float myValue;

		public Bidder(RobotController controller, float value) {
			myBidder = controller;
			myValue = value;
		}

		public RobotController bidder {
			get { return myBidder; }
		}

		public float value {
			get { return myValue; }
		}
	}

#if UNITY_EDITOR
	InspectorListElement InspectorListElement.doListElementGUI(GameObject parent) {

		TagEnum newType = (TagEnum)UnityEditor.EditorGUILayout.Popup((int)type, Enum.GetNames(typeof(TagEnum)));
		if (newType != type) {
			return constructTag(newType);
		}
		doGUI(parent);
		return this;
	}

    public virtual void drawGizmo(Label label) {

    }

	public virtual void doGUI(GameObject parent) {
		UnityEditor.EditorGUILayout.LabelField("Executors: " + getAllExecutions());
		severity = UnityEditor.EditorGUILayout.FloatField("Severity: ", severity);
	}

	private int getAllExecutions() {
		int result = 0;
		if (executors != null) {
			foreach (HashSet<RobotController> hashSet in executors.Values) {
				result += hashSet.Count;
			}
		}
		return result;
	}
#endif
}