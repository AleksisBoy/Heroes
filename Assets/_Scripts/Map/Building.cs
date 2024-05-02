using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MapObject
{
    [SerializeField] private Transform entrance = null;

    protected Player playerControlled = null;
    public Transform EntrancePreSetup { get { return entrance; } }
    
    public override void Setup(Point entrancePoint)
    {
        this.currentLocation = entrancePoint;
        if (!entrance) currentLocation.occupied = true;
    }
    public override void Interact(HeroMount hero)
    {
        if(playerControlled != hero.Player)
        {
            playerControlled = hero.Player;
            GetComponent<MeshRenderer>().material.color = playerControlled.PlayerColor;
        }
    }
}
