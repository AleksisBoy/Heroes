using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private CombatMap map = null;

    [SerializeField] private Player attackingPlayer;
    [SerializeField] private Player defendingPlayer;
    [SerializeField] private HeroMount attackingHero = null;
    [SerializeField] private HeroMount defendingHero = null;

    private CombatUnit actingUnit = null;
    private List<CombatUnit> combatUnits = new List<CombatUnit>();

    private CombatState state = CombatState.Preparation;

    private int playersReady = 0;
    private bool waitingForResponse = false;
    private float time = 0f;
    public float Time => time;

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

        map.DeactivateTiles();
        // do accordingly
        if (state == CombatState.Combat)
        {
            map.AssignAndShowPreparedUnits();
            combatUnits = map.GetAllUnits();
            foreach (CombatUnit unit in combatUnits)
            {
                unit.ATB = Random.Range(0f, 0.25f);
            }

            attackingPlayer.StartCombatMainState(map);
            defendingPlayer.StartCombatMainState(map);

            ProgressATB();
        }
        else if(state == CombatState.Ending)
        {
            attackingPlayer.StartCombatEndingState();
            defendingPlayer.StartCombatEndingState();
        }
        else
        {
            Debug.LogError("ERROR COMBAT STATE");
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
                PlayerTurnSetup();
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
                PlayerTurnSetup();
            }
        }
    }

    private void PlayerTurnSetup()
    {
        Player playerActing = actingUnit.Container.Player;
        CombatTile actingUnitTile = map.GetUnitTile(actingUnit.Container);
        List<CombatTile> activeTiles = map.GetTilesAroundTile(actingUnitTile, actingUnit.Container.Data.Speed);

        activeTiles.AddRange(map.GetEnemyUnitsTilesFor(playerActing, activeTiles));
        activeTiles.Remove(actingUnitTile);
        playerActing.CombatTurnSetup(map, activeTiles);

        StartCoroutine(WaitForResponse(playerActing, activeTiles));
    }

    private IEnumerator WaitForResponse(Player player, List<CombatTile> activeTiles)
    {
        waitingForResponse = true;
        var playerEnum = player.CombatTurnInput();
        CombatTile selectedTile = null;
        Vector2 pressDirection = Vector2.zero;
        while (playerEnum.MoveNext())
        {
            var input = playerEnum.Current;
            if(input == null)
            {
                yield return null;
                continue;
            }
            selectedTile = input.selectedTile;
            pressDirection = input.direction;
            break;
        }
        CombatTile previousTile = map.GetUnitTile(actingUnit.Container);
        previousTile.ClearTile();
        if (selectedTile.Unit && selectedTile.Unit.IsOpponent(actingUnit))
        {
            AttackUnit(actingUnit, selectedTile.Unit, pressDirection, activeTiles);
        }
        else
        {
            selectedTile.SetUnit(actingUnit);
            selectedTile.UpdateUnitTransform();
        }

        actingUnit.ATB = Random.Range(0f, 0.25f); // should depend on luck or morale?
        actingUnit = null;
        ProgressATB();
    }

    private void AttackUnit(CombatUnit attackingUnit, CombatUnit defendingUnit, Vector2 pressDirection, List<CombatTile> activeTiles)
    {
        CombatTile adjacentTile = map.GetAdjacentTileInDirectionWithin(map.GetUnitTile(defendingUnit.Container),activeTiles, pressDirection);
        
        adjacentTile.SetUnit(attackingUnit);
        adjacentTile.UpdateUnitTransform();

        int randomDamage = Random.Range(attackingUnit.Container.Data.DamageRange.x, attackingUnit.Container.Data.DamageRange.y + 1);
        int damage = randomDamage * attackingUnit.Container.Count;

        int defendersLeft = defendingUnit.TakeDamage(damage);
        if(defendersLeft <= 0)
        {
            DestroyUnit(defendingUnit);
            CheckCombatState();
        }
        else
        {
            // if defenders can they do counterattack
        }
    }
    private void DestroyUnit(CombatUnit unit)
    {
        if(combatUnits.Contains(unit)) combatUnits.Remove(unit);
        map.RemoveUnit(unit);
    }
    private void CheckCombatState()
    {
        if (map.AttackerUnits.Count == 0) FinishCombatMainState(defendingPlayer);
        else if (map.DefenderUnits.Count == 0) FinishCombatMainState(attackingPlayer);
    }
    private void FinishCombatMainState(Player winner)
    {
        Debug.Log("Combat won by " + winner.name);
        ProceedToNextState();
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
public class CombatPlayerTurnInput
{
    public CombatTile selectedTile;
    public Vector2 direction;

    public CombatPlayerTurnInput(CombatTile s, Vector2 d)
    {
        selectedTile = s;
        direction = d;
    }
}