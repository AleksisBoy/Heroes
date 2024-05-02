using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapObject : ManageableBehaviour
{
    protected Point currentLocation = null;
    public Point CurrentLocation { get { return currentLocation; } }
    public virtual void Setup(Point startPoint)
    {
        currentLocation = startPoint;
    }
    public virtual void Interact(HeroMount hero) // point action call
    {
        Debug.Log(hero.name + " interacted with " + name);
    }
    public override IEnumerator DayStarted(int day)
    {
        Debug.Log("Day " + day + " started for " + name);
        yield break;
    }
}
