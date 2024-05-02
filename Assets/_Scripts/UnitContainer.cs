using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitContainer
{
    private Player player;
    private Unit unit;
    private int count = 0;
    public Unit Data => unit;
    public int Count => count;
    public Player Player => player;

    public UnitContainer(Unit unit, int count, Player player)
    {
        this.unit = unit;
        this.count = count;
        this.player = player;
    }
    public string DebugName { get { return unit.name + " " + count; } }
}
