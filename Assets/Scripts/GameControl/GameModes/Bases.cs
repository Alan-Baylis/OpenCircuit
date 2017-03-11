using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bases : TeamGameMode {

    void Start() {
        teams.Add(new TeamData(0, new Color(0, 0, .4777f, 1)));
        teams.Add(new TeamData(1, new Color(.4777f, 0, 0, 1)));
    }

	public override bool winConditionMet() {
		return false;
	}
}
