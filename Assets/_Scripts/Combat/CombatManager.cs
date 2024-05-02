using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private CombatMap map = null;

    [SerializeField] private Player attackingPlayer;
    [SerializeField] private Player defendingPlayer;
    [SerializeField] private HeroMount attackingHero = null;
    [SerializeField] private HeroMount defendingHero = null;

    //[SerializeField] private CombatPreparation combatPreparation = null;
    //[SerializeField] private CombatMainState combatMainState = null;

    private CombatUnit actingUnit = null;
    private List<CombatUnit> combatUnits = new List<CombatUnit>();

    private CombatState state = CombatState.Preparation;

    private int playersReady = 0;
    private bool waitingForResponse = false;
    private float time = 0f;

    public void StartMatch(HeroMount attacker, HeroMount defender)
    {
        state = CombatState.Preparation;

        defender.SpawnStarterUnit();
        attacker.SpawnStarterUnit();

        attackingPlayer.StartCombatPreparation(this, attacker, map, true);
        defendingPlayer.StartCombatPreparation(this, defender, map, false);
        //combatPreparation.StartPreparation(this, attacker);
        //combatPreparation.StartPreparation(this, defender);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            StartMatch(attackingHero, defendingHero);
        }
    }
    public void PlayerReady()
    {
        playersReady++;
        if (playersReady >= 2)
        {
            ProceedToNextState();
        }
    }
    private void ProceedToNextState()
    {
        state += 1;
        // do accordingly
        if(state == CombatState.Combat)
        {
            map.AssignAndShowPreparedUnits();
            map.DeactivateTiles();
            attackingPlayer.StartCombatMainState(map);
            defendingPlayer.StartCombatMainState(map);

            combatUnits = map.GetAllUnits();
            foreach (CombatUnit unit in combatUnits)
            {
                unit.ATB = Random.Range(0f, 0.25f);
            }
            ProgressATB();
        }
    }


    // Combat Main State
    private void ProgressATB()
    {
        foreach (CombatUnit unit in combatUnits)
        {
            if (unit.ATB >= 1f && actingUnit == null)
            {
                actingUnit = unit;
                Player playerActing = actingUnit.Container.Player;
                playerActing.CombatTurnSetup(map, map.GetTilesAroundTile(map.GetUnitTile(actingUnit.Container), actingUnit.Container.Data.Speed));
                StartCoroutine(WaitForResponse(playerActing));
                return;
            }
        }
        while (actingUnit == null)
        {
            combatUnits = combatUnits.OrderByDescending(x => x.ATB).ToList();
            foreach (CombatUnit unit in combatUnits)
            {
                unit.UpdateATB(0.1f);
                if (unit.ATB >= 1f && actingUnit == null)
                {
                    actingUnit = unit;
                }
            }
            time += 0.1f;
            if (actingUnit)
            {
                Player playerActing = actingUnit.Container.Player;
                playerActing.CombatTurnSetup(map, map.GetTilesAroundTile(map.GetUnitTile(actingUnit.Container), actingUnit.Container.Data.Speed));
                StartCoroutine(WaitForResponse(playerActing));
            }
        }
    }
    private IEnumerator WaitForResponse(Player player)
    {
        waitingForResponse = true;
        var playerEnum = player.CombatTurnInput();
        CombatTile selectedTile = null;
        while (playerEnum.MoveNext())
        {
            selectedTile = playerEnum.Current;
            if (selectedTile == null)
            {
                yield return null;
                continue;
            }
            break;
        }
        CombatTile previousTile = map.GetUnitTile(actingUnit.Container);
        previousTile.ClearTile();
        selectedTile.SetUnit(actingUnit);
        selectedTile.UpdateUnitTransform();

        actingUnit.ATB = Random.Range(0f, 0.25f); // should depend on luck or morale?
        actingUnit = null;
        ProgressATB();
    }
    //private Player GetUnitsPlayer(CombatUnit unit)
    //{
    //    foreach(UnitContainer unitContainer in attackingHero.Units)
    //    {
    //        if(unitContainer == unit.Container)
    //        {
    //            return attackingPlayer;
    //        }
    //    }
    //    foreach(UnitContainer unitContainer in defendingHero.Units)
    //    {
    //        if(unitContainer == unit.Container)
    //        {
    //            return defendingPlayer;
    //        }
    //    }

    //    return null;
    //}
}
public enum CombatState
{
    Preparation,
    Combat,
    Ending
}
