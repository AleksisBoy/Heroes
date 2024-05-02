using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtificialPlayer : Player
{
    public override void StartCombatMainState(CombatMap combatMap)
    {
        if (currentState.activeSelf) currentState.SetActive(false); 
        combatMainState.StartCombat(combatMap);
        currentState = combatMainState.gameObject;
    }
    public override void CombatTurnSetup(CombatMap combatMap, List<CombatTile> activeTiles)
    {
        combatMainState.SetActive(activeTiles, true);
    }
    public override IEnumerator<CombatTile> CombatTurnInput()
    {
        bool madeTurn = false;
        while (!madeTurn)
        {
            CombatTile selectedTile = combatMainState.GetSelectedTile();
            if (selectedTile != null)
            {
                madeTurn = true;
                combatMainState.SetActive(null, false);
                combatMainState.DeactivateTiles();
                yield return selectedTile;
            }
            yield return null;
        }
    }
}
