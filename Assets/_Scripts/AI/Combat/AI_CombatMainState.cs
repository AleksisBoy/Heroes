using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_CombatMainState : CombatMainState
{
    private List<CombatTile> enemyUnitsTiles = new List<CombatTile>();
    private CombatTile closestEnemyUnitTile;
    private Node.Status treeStatus = Node.Status.RUNNING;
    private BehaviourTree tree;
    protected override void Update()
    {
        // nothing
    }
    public void StartCombat(CombatMap map, Player player, List<CombatTile> enemyUnitsTiles)
    {
        this.map = map;
        isActive = false;
        this.enemyUnitsTiles = enemyUnitsTiles;

        tree = new BehaviourTree("AI_CombatMainState");

        //Leaf moveToTile = new Leaf("Move unit to tile", MoveToTile);
        Leaf attackUnit = new Leaf("Attack unit", AttackUnit);
        Leaf canGetToEnemyUnit = new Leaf("Can Get to closest enemy?", CanGetToClosestEnemy);
        Leaf assignEnemyUnit = new Leaf("Assign closest enemy unit", AssignClosestEnemyUnit);
        Leaf goToClosestTileToEnemy = new Leaf("Go to closest Tile to enemy", GoToClosestTileToEnemy);
        Inverter cannotGetToEnemyUnit = new Inverter("Cannot get to closestEnemy?");
        cannotGetToEnemyUnit.AddChild(canGetToEnemyUnit);

        Sequence s1 = new Sequence("s1");
        s1.AddChild(assignEnemyUnit);
        s1.AddChild(canGetToEnemyUnit);
        s1.AddChild(attackUnit);

        Sequence s2 = new Sequence("s2");
        s2.AddChild(cannotGetToEnemyUnit);
        s2.AddChild(goToClosestTileToEnemy);

        Selector selecting = new Selector("sele");
        selecting.AddChild(s1);
        selecting.AddChild(s2);

        tree.AddChild(selecting);
        tree.PrintTree();

        gameObject.SetActive(true);
        StartCoroutine(Behave());
    }
    private Node.Status AssignClosestEnemyUnit()
    {
        closestEnemyUnitTile = null;
        float closestDistance = 10000f;
        foreach(CombatTile unitTile in enemyUnitsTiles)
        {
            if (!unitTile) continue;
            float distance = Vector3.Distance(unitTile.transform.position, actingUnitTile.transform.position);
            if (distance < closestDistance)
            {
                closestEnemyUnitTile = unitTile;
                closestDistance = distance;
            }
        }
        return closestEnemyUnitTile != null ? Node.Status.SUCCESS : Node.Status.FAILURE;
    }
    private Node.Status CanGetToClosestEnemy()
    {
        return activeTiles.Contains(closestEnemyUnitTile) ? Node.Status.SUCCESS : Node.Status.FAILURE;
    }
    private Node.Status AttackUnit()
    {
        Vector3 direction3D = (ActingUnitTile.Unit.transform.position - closestEnemyUnitTile.transform.position).normalized;
        Vector2 direction2D = new Vector2(direction3D.x, direction3D.z);
        playerInput = CombatPlayerTurnInput.Attack(closestEnemyUnitTile, direction2D);
        return Node.Status.SUCCESS;
    }
    private Node.Status GoToClosestTileToEnemy()
    {
        CombatTile closestTileToEnemy = map.GetClosestTileToTile(actingUnitTile, closestEnemyUnitTile, activeTiles);
        playerInput = CombatPlayerTurnInput.Move(closestTileToEnemy);
        return Node.Status.SUCCESS;
    }
    private IEnumerator Behave()
    {
        while (tree != null)
        {
            if (isActive) treeStatus = tree.Process();
            yield return new WaitForSeconds(0.1f);
        }
    }
    public void SetActive(List<CombatTile> activeTiles, bool state, CombatTile actingUnitTile, List<CombatTile> enemyUnitsTiles)
    {
        this.activeTiles = activeTiles;
        this.actingUnitTile = actingUnitTile;
        this.enemyUnitsTiles = enemyUnitsTiles;
        closestEnemyUnitTile = null;
        isActive = state;
        playerInput = null;
    }
}
