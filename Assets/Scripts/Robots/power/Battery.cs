using UnityEngine;
using System.Collections;
using System;

[AddComponentMenu("Scripts/Robot/Battery")]
public class Battery : AbstractPowerSource {

	public float maximumCapacity = 100;
	public float currentCapacity = 100;

	public override bool drawPower(float amount){
		if (currentCapacity < amount){
			return false;
		}
	   currentCapacity = currentCapacity - amount;
		return base.drawPower(amount) && true;
	}
    public void addPower (float amount) { 
        if (currentCapacity < maximumCapacity) {
            currentCapacity = currentCapacity + amount;
        }
    }

    public override bool hasPower(float amount) {
        if (currentCapacity < amount) {
            return false;
        }
        return !isDisabled;
    }
}