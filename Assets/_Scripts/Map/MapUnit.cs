using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapUnit : Building 
{
    [SerializeField] private List<NeutralUnit> units = new List<NeutralUnit>();

    public override void Interact(HeroMount hero)
    {
        ArtificialPlayer aiPlayer = Instantiate(InternalSettings.Get.ArtificialPlayer);
        UnitContainer[] unitContainers = new UnitContainer[7];
        for(int i = 0; i < unitContainers.Length; i++)
        {
            if (i > units.Count - 1) break;
            UnitContainer unit = new UnitContainer(units[i].unit, units[i].amount, aiPlayer);
            unitContainers[i] = unit;
        }
        HeroMount neutralHero = InternalSettings.Get.NeutralHero;
        neutralHero.Debug_SetPlayer(aiPlayer);
        neutralHero.Debug_SetUnits(unitContainers);
        aiPlayer.AddHeroMount(neutralHero);
        StartCoroutine(GameManager.Instance.StartCombatSequence(hero, neutralHero));
    }
}
[Serializable]
public struct NeutralUnit
{
    public Unit unit;
    public int amount;
}
