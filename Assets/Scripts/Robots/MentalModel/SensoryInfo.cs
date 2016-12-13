using UnityEngine;
using System.Collections.Generic;

public class SensoryInfo {

	private int sensorCount;
	private Vector3 position;
	private Vector3? direction;
	private System.DateTime lastSighting;

	private List<Tag> attachedTags;

	public SensoryInfo(Vector3 position, Vector3? direction, System.DateTime time, List<Tag> tags, int sightingCount) {
		this.sensorCount = sightingCount;
		this.position = position;
		this.direction = direction;
		this.lastSighting = time;
		this.attachedTags = tags;
		
	}

	public void updateInfo(LabelHandle target) {
		if (getSightings() > 0) {
			updatePosition(target.getPosition());
			updateTime(System.DateTime.Now);
			updateAttachedTags(target.getTags());
			if (target.getDirection() != null) {
				updateDirection(target.getDirection());
			}
		}
	}

	public void updateInfo(Vector3 position, System.DateTime lastSeenTime, Vector3? direction) {
		updatePosition(position);
		updateTime(lastSeenTime);
		if (direction != null) {
			updateDirection(direction);
		}
	}

	public List<Tag> getAttachedTags() {
		return attachedTags;
	}

	public void addSighting() {
		++sensorCount;
	}

	public void removeSighting() {
		--sensorCount;
	}

	public int getSightings() {
		return sensorCount;
	}

	public Vector3 getPosition() {
		return position;
	}

	public Vector3? getDirection() {
		return direction;
	}

	public System.DateTime getSightingTime() {
		return lastSighting;
	}

	private void updateAttachedTags(List<Tag> tags) {
		attachedTags = tags;
	}

	private void updatePosition(Vector3 pos) {
		this.position = pos;
	}

	private void updateDirection(Vector3? dir) {
		this.direction = dir;
	}

	private void updateTime(System.DateTime time) {
		this.lastSighting = time;
	}
}
