using System;
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

    public CombatMap Map => map;
    private CombatUnit actingUnit = null;
    private List<CombatUnit> combatUnits = new List<CombatUnit>();

    private Action onProgressATB = null;
    public Action OnProgressATB { get {  return onProgressATB; }  set { onProgressATB = value; } }

    private Action<CombatUnit> onUnitDestroy = null;
    public Action<CombatUnit> OnUnitDestroy { get {  return onUnitDestroy; }  set { onUnitDestroy = value; } }

    private Dictionary<Unit, int> defenderCasualties = new Dictionary<Unit, int>();
    private Dictionary<Unit, int> attackerCasualties = new Dictionary<Unit, int>();

    private CombatState state = CombatState.Preparation;
    private Player winner = null;

    private int playersReady = 0;
    private bool waitingForResponse = false;
    private float time = 0f;
    private int round = 0;
    public float GameTime => time;
    public int Round => round;

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
                unit.ForceSetATB(UnityEngine.Random.Range(0f, 0.25f));
                unit.retaliate = true;
            }
            attackingPlayer.StartCombatMainState(this);
            defendingPlayer.StartCombatMainState(this);

            ProgressATB();

        }
        else if(state == CombatState.Ending)
        {
            List<UnitContainer> casualtiesAttacker = ConvertToContainerList(attackerCasualties);
            List<UnitContainer> casualtiesDefender = ConvertToContainerList(defenderCasualties);

            attackingPlayer.StartCombatEndingState(winner, casualtiesAttacker, casualtiesDefender);
            defendingPlayer.StartCombatEndingState(winner, casualtiesDefender, casualtiesAttacker);
            Debug.Log("ending");
        }
        else
        {
            Debug.LogError("ERROR COMBAT STATE");
        }
    }


    // Combat Main State
    private void ProgressATB()
    {
        CombatUnit fastestUnit = GetFastestUnitOnMap(out float remainTime);
        while (actingUnit == null)
        {
            combatUnits = combatUnits.OrderByDescending(x => x.ATB).ToList();
            foreach (CombatUnit unit in combatUnits)
            {
                unit.UpdateATB(remainTime);
                if (unit.ATB >= 1f && actingUnit == null)
                {
                    actingUnit = unit;
                }
            }

            time += remainTime;
            if(time >= 1f)
            {
                round++;
                time -= 1f;
                foreach(CombatUnit unit in combatUnits)
                {
                    unit.retaliate = true;
                }
            }
        }
        if(OnProgressATB != null) OnProgressATB();
        PlayerTurnSetup();
    }
    private void PlayerTurnSetup()
    {
        Player playerActing = actingUnit.Container.Player;
        CombatTile actingUnitTile = map.GetUnitTile(actingUnit.Container);
        List<CombatTile> activeTiles = map.GetTilesAroundTile(actingUnitTile, actingUnit.Container.Data.Speed);

        List<CombatTile> enemyUnitTiles = map.GetEnemyUnitsTilesOnBorderForPlayer(playerActing, activeTiles);
        activeTiles.AddRange(enemyUnitTiles);
        activeTiles.Remove(actingUnitTile);
        playerActing.CombatTurnSetup(map, activeTiles, enemyUnitTiles, actingUnitTile);

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
            yield return MoveUnit(actingUnit, selectedTile, activeTiles);
        }

        actingUnit.ResetATB();
        actingUnit = null;
        ProgressATB();
    }
    private IEnumerator MoveUnit(CombatUnit unit, CombatTile tile, List<CombatTile> tileRange)
    {
        CombatTile previousTile = map.GetUnitTile(unit.Container);
        previousTile.ClearTile();

        unit.Visual.Animator.SetFloat("Speed01", 1f);
        Stack<CombatTile> path = map.GetPathToTile(previousTile, tile, tileRange);
        while(path.Count > 0)
        {
            CombatTile nextTile = path.Pop();

            Vector3 startPos = unit.transform.position;
            Vector3 direction = nextTile.transform.position - unit.transform.position;
            direction.y = 0f;
            unit.transform.forward = direction.normalized;
            float power = 0f;
            while(power < 1f)
            {
                unit.transform.position = Vector3.Lerp(startPos, nextTile.transform.position, power);
                unit.OnUnitUpdateUI();
                power += Time.deltaTime * unit.Visual.MoveSpeed;
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
        CombatTile adjacentTile = map.GetAdjacentTileInDirectionWithin(map.GetUnitTile(defendingUnit.Container), activeTiles, pressDirection, map.GetUnitTile(attackingUnit.Container), out float angle);

        yield return MoveUnit(attackingUnit, adjacentTile, activeTiles);

        attackingUnit.transform.forward = defendingUnit.transform.position - attackingUnit.transform.position;
        defendingUnit.transform.forward = attackingUnit.transform.position - defendingUnit.transform.position;

        int defendersLeft = UnitAttack(attackingUnit, defendingUnit);

        yield return new WaitForSeconds(.5f);
        if (defendersLeft <= 0)
        {
            // attack done with defender dying
            KillUnit(attackingUnit, defendingUnit);
            yield break;
        }

        if (defendingUnit.retaliate)
        {
            defendingUnit.retaliate = false;

            int attackersLeft = UnitAttack(defendingUnit, attackingUnit);

            yield return new WaitForSeconds(.5f);
            if (attackersLeft <= 0)
            {
                // attack done with attacker dying
                KillUnit(defendingUnit, attackingUnit);
                yield break;
            }
        }

        // attack done with noone dying
        map.GetUnitTile(attackingUnit.Container).UpdateUnitRotation();
        map.GetUnitTile(defendingUnit.Container).UpdateUnitRotation();
    }
    private int UnitAttack(CombatUnit attackingUnit, CombatUnit defendingUnit)
    {
        int attackingDamage = GetDamage(attackingUnit, defendingUnit);
        Debug.Log("attacking " + defendingUnit.name + " with damage " + attackingDamage);

        attackingUnit.Visual.Animator.SetTrigger("Attack");
        int defendersCount = defendingUnit.Container.Count;
        int defendersLeft = defendingUnit.TakeDamage(attackingDamage);

        int defendersLost = defendersCount - defendersLeft;
        if (defendersLost > 0) SaveCasualtyForPlayer(defendingUnit.Container.Data, defendersLost, defendingUnit.Container.Player);

        return defendersLeft;
    }
    private void KillUnit(CombatUnit attackingUnit, CombatUnit defendingUnit)
    {
        map.GetUnitTile(attackingUnit.Container).UpdateUnitRotation();
        DestroyUnit(defendingUnit);
        CheckCombatState();
    }
    private static int GetDamage(CombatUnit attackingUnit, CombatUnit defendingUnit)
    {
        int randomDamage = UnityEngine.Random.Range(attackingUnit.Container.Data.DamageRange.x, attackingUnit.Container.Data.DamageRange.y + 1);
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
    public static Vector2Int GetDamageRange(CombatUnit attackingUnit, CombatUnit defendingUnit)
    {
        Vector2Int damageRange = new Vector2Int();
        int damageLow = attackingUnit.Container.Count * attackingUnit.Container.Data.DamageRange.x;
        int damageHigh = attackingUnit.Container.Count * attackingUnit.Container.Data.DamageRange.y;

        if (attackingUnit.Container.Data.Attack >= defendingUnit.Container.Data.Defense)
        {
            damageLow = (int)Mathf.Floor(damageLow * (1 + 0.05f * (attackingUnit.Container.Data.Attack - defendingUnit.Container.Data.Defense)));
            damageHigh = (int)Mathf.Floor(damageHigh * (1 + 0.05f * (attackingUnit.Container.Data.Attack - defendingUnit.Container.Data.Defense)));
        }
        else
        {
            damageLow = (int)Mathf.Floor(damageLow / (1 + 0.05f * (defendingUnit.Container.Data.Defense - attackingUnit.Container.Data.Attack)));
            damageHigh = (int)Mathf.Floor(damageHigh / (1 + 0.05f * (defendingUnit.Container.Data.Defense - attackingUnit.Container.Data.Attack)));
        }
        damageRange.x = damageLow;
        damageRange.y = damageHigh;
        return damageRange;
    }
    private void DestroyUnit(CombatUnit unit)
    {
        if(OnUnitDestroy != null)OnUnitDestroy(unit);
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
        this.winner = winner;
        Debug.Log("Combat won by " + winner.name);
        ProceedToNextState();
    }
    private void SaveCasualtyForPlayer(Unit unit, int count, Player player)
    {
        Dictionary<Unit, int> dic;
        if (player == attackingPlayer) dic = attackerCasualties;
        else if (player == defendingPlayer) dic = defenderCasualties;
        else
        {
            Debug.LogError("NO SUCH PLAYER FOR REGISTERING CASUALTIES");
            return;
        }

        if(!dic.TryAdd(unit, count))
        {
            dic[unit] += count;
        }
    }
    private List<UnitContainer> ConvertToContainerList(Dictionary<Unit, int> casualties)
    {
        List<UnitContainer> units = new List<UnitContainer>();
        foreach(Unit unit in casualties.Keys)
        {
            UnitContainer container = new UnitContainer(unit, casualties[unit], null);
            units.Add(container);
        }
        return units;
    }
    public List<CombatUnit> GetCombatUnits()
    {
        return new List<CombatUnit>(combatUnits);
    }
    public static float GetRemainToATB(CombatUnit unit, int cycle)
    {
        return (Mathf.Clamp01(1f - unit.ATB) + cycle) / (unit.Container.Data.Initiative / 10f);
    }
    private CombatUnit GetFastestUnitOnMap(out float remainAtb)
    {
        float lowestValue = 10f;
        CombatUnit fastestUnit = null;
        foreach (CombatUnit unit in combatUnits)
        {
            float remainToATB = GetRemainToATB(unit, 0);
            if (remainToATB < lowestValue)
            {
                lowestValue = remainToATB;
                fastestUnit = unit;
            }
        }
        remainAtb = lowestValue;
        return fastestUnit;
    }
    public Player GetPlayer(CombatUnit unit)
    {
        if (map.AttackerUnits.Contains(unit)) return attackingPlayer;
        if (map.DefenderUnits.Contains(unit)) return defendingPlayer;

        return null;
    }
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