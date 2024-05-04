using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_CombatMainState : CombatMainState
{
    private List<CombatUnit> enemyUnits = new List<CombatUnit>();
    private Node.Status treeStatus = Node.Status.RUNNING;
    private BehaviourTree tree;
    protected override void Update()
    {
        // nothing
    }
    public override void StartCombat(CombatMap map, Player player)
    {
        this.map = map;
        isActive = false;

        tree = new BehaviourTree();

        Leaf moveToTile = new Leaf("Move unit to tile", MoveToTile);
        //Leaf attackUnit = new Leaf("Attack unit", );

        tree.AddChild(moveToTile);



        gameObject.SetActive(true);
        StartCoroutine(Behave());
    }
    private Node.Status MoveToTile()
    {
        // needs to know the unit its supposed to move
        // all enemy(player's) units
        // calculate all possible attacks and prioritize them by efficiency of the attack
        // make a move with the most efficient attack or move
        // move would be towards the closest unit or unit that is most dangerous in enemy pool
        selectedTile = map.GetRandomFreeTile(activeTiles);
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

}
