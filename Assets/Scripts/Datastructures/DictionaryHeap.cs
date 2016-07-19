using System.Collections.Generic;
using System;

class DictionaryHeap {
    SortedDictionary<Prioritizable, Prioritizable> list = new SortedDictionary<Prioritizable, Prioritizable>(new PrioritizableComparer());

    class PrioritizableComparer : IComparer<Prioritizable> {


        public int Compare(Prioritizable a, Prioritizable b) {
            float aPriority = a.getPriority();
            float bPriority = b.getPriority();
            if (aPriority < bPriority) {
                return 1;
            } else if (aPriority > bPriority) {
                return -1;
            } else return 0;
        }
    }

    int count = 0;

    public void Enqueue(Prioritizable p) {
        if (p == null) {
            throw new NullReferenceException("PriorityQueue does not accept null objects");
        }
        list.Add(p, p);
        count++;
    }

    public Prioritizable Dequeue() {
        Prioritizable max = null;
        foreach (KeyValuePair<Prioritizable, Prioritizable> kvp in list) {
            max = kvp.Key;
            break;
        }
        list.Remove(max);
        count--;
        return max;
    }

    public Prioritizable peek() {
        Prioritizable result = null;
        foreach (KeyValuePair<Prioritizable, Prioritizable> kvp in list) {
            result = kvp.Key;
            break;
        }
        return result;
    }

    public int Count {
        get {
            return count;
        }
    }

}

