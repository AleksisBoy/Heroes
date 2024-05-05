using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtificialPlayer : Player
{
    public override void StartCombatMainState(CombatMap combatMap, bool attacker)
    {
        AI_CombatMainState aI_CombatMainState = (AI_CombatMainState)combatMainState;
        if(!aI_CombatMainState)
        {
            Debug.LogError("AI COMBAT MAIN STATE WAS NOT SET FOR AI");
            return;
        }
        if (currentState.activeSelf) currentState.SetActive(false);
        aI_CombatMainState.StartCombat(combatMap, this, combatMap.GetEnemyUnitTiles(this));
        currentState = combatMainState.gameObject;
    }
    public override void CombatTurnSetup(CombatMap combatMap, List<CombatTile> activeTiles, List<CombatTile> enemyTiles, CombatTile actingUnitTile)
    {
        AI_CombatMainState aI_CombatMainState = (AI_CombatMainState)combatMainState;
        if (!aI_CombatMainState)
        {
            Debug.LogError("AI COMBAT MAIN STATE WAS NOT SET FOR AI");
            return;
        }
        aI_CombatMainState.SetActive(activeTiles, true, actingUnitTile, combatMap.GetEnemyUnitTiles(this));
    }
    public override IEnumerator<CombatPlayerTurnInput> CombatTurnInput()
    {
        bool madeTurn = false;
        while (!madeTurn)
        {
            CombatTile selectedTile = combatMainState.GetSelectedTile();
            if (selectedTile != null)
            {
                CombatPlayerTurnInput input = new CombatPlayerTurnInput(selectedTile, (selectedTile.transform.position - combatMainState.ActingUnitTile.Unit.transform.position).normalized);
                madeTurn = true;
                combatMainState.SetActive(null, false, null);
                combatMainState.DeactivateTiles();
                yield return input;
            }
            yield return null;
        }
    }
}
