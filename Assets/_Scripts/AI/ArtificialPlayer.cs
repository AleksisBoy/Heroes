using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtificialPlayer : Player
{
    public override void StartCombatMainState(CombatMap combatMap)
    {
        if (currentState.activeSelf) currentState.SetActive(false); 
        combatMainState.StartCombat(combatMap, this);
        currentState = combatMainState.gameObject;
    }
    public override void CombatTurnSetup(CombatMap combatMap, List<CombatTile> activeTiles, List<CombatTile> enemyTiles, CombatTile actingUnitTile)
    {
        combatMainState.SetActive(activeTiles, true, actingUnitTile);
    }
    public override IEnumerator<CombatPlayerTurnInput> CombatTurnInput()
    {
        bool madeTurn = false;
        while (!madeTurn)
        {
            CombatTile selectedTile = combatMainState.GetSelectedTile();
            if (selectedTile != null)
            {
                madeTurn = true;
                combatMainState.SetActive(null, false, null);
                combatMainState.DeactivateTiles();
                yield return new CombatPlayerTurnInput(selectedTile, Vector2.zero);
            }
            yield return null;
        }
    }
}
