using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class EndeavourComparer : IEqualityComparer<Endeavour> {

	public bool Equals(Endeavour a, Endeavour b) {
		return a.Equals (b);
	}

	public int GetHashCode(Endeavour a) {
		int hash = 17;
		hash = hash * 31 + a.getName ().GetHashCode ();
		hash = hash * 31 + a.getPrimaryTagType().GetHashCode ();
		return hash;
	}
}
