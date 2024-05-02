using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point
{
    private Vector3 position;
    private float terrainCost;
    public bool occupied;

    private Action<HeroMount> onInteract = null;

    public Vector3 Position { get { return position; } }

    public Point (Vector3 position, float terrainCost, bool occupied)
    {
        this.position = position;
        this.terrainCost = terrainCost;
        this.occupied = occupied;
    }    
    public void AssignOnInteract(Action<HeroMount> act)
    {
        onInteract += act;
    }

    public void OnInteract(HeroMount hero)
    {
        if (onInteract != null) onInteract(hero);
    }
}
