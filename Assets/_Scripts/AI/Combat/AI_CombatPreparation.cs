using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_CombatPreparation : CombatPreparation
{
    [SerializeField] private CombatUnit unitPrefab = null;

    private CombatUnit[] unitsPlaced = new CombatUnit[7];
    private List<CombatTile> preparationTiles = new List<CombatTile>();
    private BehaviourTree tree = null;
    private Node.Status treeStatus = Node.Status.RUNNING;
    private bool attacker;
    private bool startingFromTop = false;
    private HeroMount hero;
    public override void StartPreparation(CombatManager manager, HeroMount hero, CombatMap map, bool attacker)
    {
        startingFromTop = Random.value >= 0.5f ? true : false;
        this.manager = manager;
        this.map = map;
        playerReady = false;
        this.attacker = attacker;
        this.hero = hero;
        preparationTiles = map.GetTilesByColomns(map.GetPreparationColomns(attacker));

        gameObject.SetActive(true);

        tree = new BehaviourTree("AI_CombatPreparation");

        Leaf unitsArePlaced = new Leaf("Are units placed?", AllUnitsPlaced);
        Inverter unitsNotPlaced = new Inverter("Units left to place?");
        unitsNotPlaced.AddChild(unitsArePlaced);
        BehaviourTree whileUnitsNotPlaced = new BehaviourTree("While units not placed");
        whileUnitsNotPlaced.AddChild(unitsNotPlaced);
        Loop placeUnitsOnMap = new Loop("Place Units on Map", whileUnitsNotPlaced);
        Leaf placeUnitOnMap = new Leaf("Place unit on map", PlaceUnitOnMap);
        placeUnitsOnMap.AddChild(placeUnitOnMap);
        Leaf finishPreparation = new Leaf("Finish Preparation", FinishPreparation);
        Sequence decide = new Sequence("Ai Decision");
        decide.AddChild(placeUnitsOnMap);
        decide.AddChild(finishPreparation);
        tree.AddChild(decide);

        tree.PrintTree();

        StartCoroutine(Behave());
    }
    private Node.Status AllUnitsPlaced()
    {
        for(int i = 0; i < unitsPlaced.Length; i++)
        {
            UnitContainer unitPlacedContainer = unitsPlaced[i] ? unitsPlaced[i].Container : null;
            if (unitPlacedContainer == hero.Units[i])
            {
                continue;
            }
            return Node.Status.FAILURE;
        }
        return Node.Status.SUCCESS;
    }
    private Node.Status PlaceUnitOnMap()
    {
        UnitContainer unitToPlace = null;
        int unitAmount = 0;
        int unitIndex = 0;
        for (int i = 0; i < unitsPlaced.Length; i++)
        {
            UnitContainer unitPlacedContainer = unitsPlaced[i] ? unitsPlaced[i].Container : null;
            if (unitPlacedContainer == hero.Units[i])
            {
                if (unitPlacedContainer != null) unitAmount++;
                continue;
            }
            unitToPlace = hero.Units[i];
            unitIndex = i;
            unitAmount++;
            break;
        }
        if (unitToPlace == null) return Node.Status.FAILURE;

        int unitCount = hero.GetUnitCount();
        CombatUnit unitPlaced = InstantiateUnit(unitToPlace);
        CombatTile tileForUnit = SelectTileForUnit(unitAmount, unitCount);

        if (attacker) map.AddAttackerUnitOnMapPrepare(unitPlaced, tileForUnit);
        else map.AddDefenderUnitOnMapPrepare(unitPlaced, tileForUnit);

        unitsPlaced[unitIndex] = unitPlaced;

        return Node.Status.SUCCESS;
    }

    private CombatUnit InstantiateUnit(UnitContainer unitToPlace)
    {
        CombatUnit unitPlaced = Instantiate(unitPrefab);
        unitPlaced.gameObject.SetActive(false);
        unitPlaced.Set(unitToPlace, attacker);
        return unitPlaced;
    }

    private CombatTile SelectTileForUnit(int unitAmount, int unitCount)
    {
        CombatTile tileForUnit;
        int tileIndex = map.MapSize.y / unitCount * unitAmount - 1;
        if (startingFromTop) tileIndex = Mathf.Abs(tileIndex - map.MapSize.y + 1);
        if (attacker)
        {
            tileForUnit = map.GetTileInColomn(1, tileIndex);
        }
        else
        {
            tileForUnit = map.GetTileInColomn(map.MapSize.x - 2, tileIndex);
        }

        return tileForUnit;
    }

    private Node.Status FinishPreparation()
    {
        Button_PlayerReady();
        tree = null;
        return Node.Status.SUCCESS;
    }
    private IEnumerator Behave()
    {
        while (tree != null)
        {
            treeStatus = tree.Process();
            yield return new WaitForSeconds(0.1f);
        }
    }
}
public enum AIState
{
    IDLE,
    WORKING,
    ISREADY,
    ALLDONE
}
