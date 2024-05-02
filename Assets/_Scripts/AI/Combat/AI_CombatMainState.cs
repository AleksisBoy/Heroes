using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_CombatMainState : CombatMainState
{
    private Node.Status treeStatus = Node.Status.RUNNING;
    private BehaviourTree tree;
    protected override void Update()
    {
        // nothing
    }
    public override void StartCombat(CombatMap map)
    {
        this.map = map;
        isActive = false;
        gameObject.SetActive(true);

        tree = new BehaviourTree();

        Leaf moveToTile = new Leaf("Move unit to tile", MoveToTile);
        //Leaf attackUnit = new Leaf("Attack unit", );

        tree.AddChild(moveToTile);

        StartCoroutine(Behave());
    }
    private Node.Status MoveToTile()
    {
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
