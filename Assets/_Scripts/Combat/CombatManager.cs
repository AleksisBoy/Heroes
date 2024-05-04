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
    [SerializeField] private float unitSpeed = 5f;

    private CombatUnit actingUnit = null;
    private List<CombatUnit> combatUnits = new List<CombatUnit>();

    private CombatState state = CombatState.Preparation;

    private int playersReady = 0;
    private bool waitingForResponse = false;
    private float time = 0f;
    private int round = 0;
    public float GameTime => time;

    public void StartMatch(HeroMount attacker, HeroMount defender)
    {
        round = 1;
        time = 0f;
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
                unit.ResetATB(Random.Range(0f, 0.25f));
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
            if(time >= 1f)
            {
                round++;
                time -= 1f;
                foreach(CombatUnit unit in combatUnits)
                {
                    unit.retaliate = true;
                }
                Debug.Log("new round " + round);
            }
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

        if (selectedTile.Unit && selectedTile.Unit.IsOpponent(actingUnit))
        {
            yield return Attack(actingUnit, selectedTile.Unit, pressDirection, activeTiles);
        }
        else
        {
            yield return MoveUnit(actingUnit, selectedTile);
        }

        actingUnit.ResetATB(Random.Range(0f, 0.25f));
        actingUnit = null;
        ProgressATB();
    }
    private IEnumerator MoveUnit(CombatUnit unit, CombatTile tile)
    {
        CombatTile previousTile = map.GetUnitTile(unit.Container);
        previousTile.ClearTile();

        unit.Visual.Animator.SetFloat("Speed01", 1f);
        Queue<CombatTile> path = new Queue<CombatTile>();
        path.Enqueue(tile);
        while(path.Count > 0)
        {
            CombatTile nextTile = path.Dequeue();

            Vector3 startPos = unit.transform.position;
            Vector3 direction = nextTile.transform.position - unit.transform.position;
            direction.y = 0f;
            unit.transform.forward = direction.normalized;
            float power = 0f;
            while(power < 1f)
            {
                unit.transform.position = Vector3.Lerp(startPos, nextTile.transform.position, power);
                unit.OnUnitUpdateUI();
                power += Time.deltaTime * unitSpeed;
                yield return null;
            }
            unit.transform.position = nextTile.transform.position;
        }
        tile.SetUnit(unit);
        tile.UpdateUnitTransform();
        unit.Visual.Animator.SetFloat("Speed01", 0f);
        yield return null;
    }
    private IEnumerator Attack(CombatUnit attackingUnit, CombatUnit defendingUnit, Vector2 pressDirection, List<CombatTile> activeTiles)
    {
        CombatTile adjacentTile = map.GetAdjacentTileInDirectionWithin(map.GetUnitTile(defendingUnit.Container),activeTiles, pressDirection);

        yield return MoveUnit(attackingUnit, adjacentTile);

        attackingUnit.transform.forward = defendingUnit.transform.position - attackingUnit.transform.position;
        defendingUnit.transform.forward = attackingUnit.transform.position - defendingUnit.transform.position;

        int attackingDamage = GetDamage(attackingUnit, defendingUnit);
        Debug.Log("attacking " + defendingUnit.name + " with damage " + attackingDamage);

        attackingUnit.Visual.Animator.SetTrigger("Attack");
        int defendersLeft = defendingUnit.TakeDamage(attackingDamage);

        yield return new WaitForSeconds(1.5f);
        if (defendersLeft <= 0)
        {
            // attack done with defender dying
            map.GetUnitTile(attackingUnit.Container).UpdateUnitRotation();
            DestroyUnit(defendingUnit);
            CheckCombatState();
            yield break;
        }

        if (defendingUnit.retaliate)
        {
            defendingUnit.retaliate = false;
            int retaliatingDamage = GetDamage(defendingUnit, attackingUnit);
            Debug.Log("retaliating on " + attackingUnit.name + " with damage " + retaliatingDamage);

            defendingUnit.Visual.Animator.SetTrigger("Attack");
            int attackersLeft = attackingUnit.TakeDamage(retaliatingDamage);

            yield return new WaitForSeconds(1.5f);
            if (attackersLeft <= 0)
            {
                // attack done with attacker dying
                map.GetUnitTile(defendingUnit.Container).UpdateUnitRotation();
                DestroyUnit(attackingUnit);
                CheckCombatState();
                yield break;
            }
        }

        // attack done with noone dying
        map.GetUnitTile(attackingUnit.Container).UpdateUnitRotation();
        map.GetUnitTile(defendingUnit.Container).UpdateUnitRotation();
    }
    private int GetDamage(CombatUnit attackingUnit, CombatUnit defendingUnit)
    {
        int randomDamage = Random.Range(attackingUnit.Container.Data.DamageRange.x, attackingUnit.Container.Data.DamageRange.y + 1);
        int damage = attackingUnit.Container.Count * randomDamage;

        if (attackingUnit.Container.Data.Attack >= defendingUnit.Container.Data.Defense)
        {
            damage = (int)Mathf.Floor(damage * (1 + 0.05f * (attackingUnit.Container.Data.Attack - defendingUnit.Container.Data.Defense)));
        }
        else
        {
            damage = (int)Mathf.Floor(damage / (1 + 0.05f * (defendingUnit.Container.Data.Defense - attackingUnit.Container.Data.Attack)));
        }
        return damage;
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