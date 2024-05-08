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
            CombatTile selectedTile = combatMainState.GetSelectedTile();
            if (selectedTile != null)
            {
                madeTurn = true;
                AI_CombatMainState aI_CombatMainState = (AI_CombatMainState)combatMainState;

                Vector3 direction3D = (combatMainState.ActingUnitTile.Unit.transform.position - selectedTile.transform.position).normalized;
                Vector2 direction2D = new Vector2(direction3D.x, direction3D.z);
                CombatPlayerTurnInput input = new CombatPlayerTurnInput(selectedTile, direction2D);

                aI_CombatMainState.SetActive(null, false, null, null);
                aI_CombatMainState.DeactivateTiles();
                yield return input;
            }
            yield return null;
        }
    }
}
