using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PileOfResources : Building
{
    [SerializeField] private ResourceType resourceType = ResourceType.Sulfur;
    [SerializeField] private int amount = 1;

    public override void Interact(HeroMount hero)
    {
        Destroy(gameObject);
        hero.Player.AddResource(resourceType, amount);
        currentLocation.occupied = false;
    }
}
