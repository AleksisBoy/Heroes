using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CombatUnit : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;

    public float ATB = 0f;
    private bool attacker = false;
    public bool Attacker => attacker;

    private Action<CombatUnit> onUnitMoved;
    private UnitContainer unit;
    public UnitContainer Container => unit;
    public void Set(UnitContainer unit, bool attacker)
    {
        this.unit = unit;
        this.attacker = attacker;
        meshFilter.mesh = unit.Data.Mesh;
        ATB = 0f;
    }
    public void UpdateATB(float time)
    {
        float initiativePerTurn = unit.Data.Initiative / 10f;
        ATB += initiativePerTurn * time;
    }
    public void OnUnitMoved()
    {
        if (onUnitMoved != null) onUnitMoved(this);
    }
    public void AddActionOnUnitMoved(Action<CombatUnit> action)
    {
        onUnitMoved += action;
    }
}
