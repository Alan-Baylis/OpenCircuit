using System.Collections.Generic;
using System;

class DictionaryHeap {
    MaxHeap<Prioritizable> list = new MaxHeap<Prioritizable>(new PrioritizableComparer());

    class PrioritizableComparer : Comparer<Prioritizable> {

		public override int Compare(Prioritizable a, Prioritizable b) {
            float aPriority = a.getPriority();
            float bPriority = b.getPriority();
            if (aPriority > bPriority) {
                return 1;
            } else if (aPriority < bPriority) {
                return -1;
            } else return 0;
        }
    }

    public void Enqueue(Prioritizable p) {
        if (p == null) {
            throw new NullReferenceException("PriorityQueue does not accept null objects");
        }
        list.Add(p);
    }

    public Prioritizable Dequeue() {
		
        return list.ExtractDominating(); ;
    }

    public Prioritizable peek() {

        return list.GetMin();
    }

    public int Count {
        get {
            return list.Count;
        }
    }

}

