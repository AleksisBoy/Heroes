using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Map map = null;
    [SerializeField] private Color playerColor = Color.red;
    [SerializeField] private List<HeroMount> heroes = new List<HeroMount>();
    [SerializeField] private CombatPreparation combatPreparation = null;
    [SerializeField] protected CombatMainState combatMainState = null;
    [SerializeField] private CombatEnding combatEnding = null;

    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
    public Action resourcesUpdated = null;
    public Color PlayerColor => playerColor;
    public List<HeroMount> Heroes => new List<HeroMount>(heroes);
    public Dictionary<ResourceType, int> Resources => new Dictionary<ResourceType, int>(resources);

    protected GameObject currentState = null;
    private void Awake()
    {
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            resources.Add(type, 0);
        }
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }
    private IEnumerator Start()
    {
        if (map == null) yield break;

        yield return null;
        foreach(HeroMount hero in heroes)
        {
            hero.Setup(map.Points[UnityEngine.Random.Range(0, 10)], this);
        }
    }
    public void CallItADay() //Button call
    {
        GameManager.Instance.PlayerFinishedDay(this);
    }
    public void AddResource(ResourceType type, int amount)
    {
        resources[type] += amount;
        if(resourcesUpdated != null) resourcesUpdated();
    }
    public virtual void StartCombatPreparation(CombatManager manager, HeroMount hero, CombatMap map, bool attacker)
    {
        if(!IsAllyHero(hero))
        {
            Debug.LogError("NO HERO " + hero.name + " FOR PLAYER " + name);
            return;
        }

        hero.Debug_SetPlayer(this);

        if (currentState != null) currentState.SetActive(false);
        combatPreparation.StartPreparation(manager, hero, map, attacker);
        currentState = combatPreparation.gameObject;
    }
    public virtual void StartCombatMainState(CombatMap combatMap, bool attacker)
    {
        if (currentState != null) currentState.SetActive(false);
        combatMainState.StartCombat(combatMap, this);
        currentState = combatMainState.gameObject;
    }
    public virtual void CombatTurnSetup(CombatMap combatMap, List<CombatTile> activeTiles, List<CombatTile> enemyTiles, CombatTile actingUnitTile)
    {
        combatMap.DeactivateTiles();
        combatMainState.SetActive(activeTiles, true, actingUnitTile);
        foreach(CombatTile tile in activeTiles)
        {
            //tile.UpdateState(CombatTile.State.Active);
            tile.AddState(CombatTile.State.Active);
        }
        foreach (CombatTile tile in enemyTiles)
        {
            //tile.UpdateState(CombatTile.State.None);
            tile.ClearStates();
        }
        //actingUnitTile.UpdateState(CombatTile.State.Selected);
        actingUnitTile.AddState(CombatTile.State.Selected);
    }
    public virtual IEnumerator<CombatPlayerTurnInput> CombatTurnInput() 
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
                yield return new CombatPlayerTurnInput(selectedTile, combatMainState.GetSelectedTileDirection());
            }
            yield return null;
        }
    }
    public virtual void StartCombatEndingState(Player winner, List<UnitContainer> myCasualties, List<UnitContainer> opponentCasualties)
    {
        if (currentState != null) currentState.SetActive(false);
        combatEnding.StartEnding(winner == this ? true : false, myCasualties, opponentCasualties);
        currentState = combatMainState.gameObject;
    }
    public bool IsAllyHero(HeroMount hero)
    {
        foreach (HeroMount mount in heroes)
        {
            if (hero == mount)
            {
                return true;
            }
        }
        return false;
    }
}
