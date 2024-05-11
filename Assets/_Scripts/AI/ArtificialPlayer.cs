using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtificialPlayer : Player
{
    public override void StartCombatMainState(CombatManager manager)
    {
        AI_CombatMainState aI_CombatMainState = (AI_CombatMainState)combatMainState;
        if(!aI_CombatMainState)
        {
            Debug.LogError("AI COMBAT MAIN STATE WAS NOT SET FOR AI");
            return;
        }
        if (currentState.activeSelf) currentState.SetActive(false);
        aI_CombatMainState.StartCombat(manager.Map, this, manager.Map.GetEnemyUnitTiles(this));
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
            CombatPlayerTurnInput playerInput = combatMainState.GetPlayerInput();
            if (playerInput != null)
            {
                madeTurn = true;
                AI_CombatMainState aI_CombatMainState = (AI_CombatMainState)combatMainState;

                aI_CombatMainState.SetActive(null, false, null, null);
                aI_CombatMainState.DeactivateTiles();
                yield return playerInput;
            }
            yield return null;
        }
    }
}
