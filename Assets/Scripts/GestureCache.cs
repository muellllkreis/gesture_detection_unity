using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureCache
{
    int defaultCapacity = 10;
    Queue<int> gestures;

    public GestureCache() {
        this.gestures = new Queue<int>(defaultCapacity);
    }

    public void Add(int gesture) {
        if(gestures.Count == defaultCapacity) {
            gestures.Dequeue();
        }
        gestures.Enqueue(gesture);
    }

    public int GetCachedGesture() {
        int[] cache = gestures.ToArray();
        int sum = 0;
        for(int i = 0; i < cache.Length; i++) {
            sum += cache[i];
        }
        return (int) (sum / cache.Length);
    }
}
