using System;
using System.Collections;
using UnityEngine;

public class CombatUnit : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;

    private int hp = 0;
    public int HP => hp;
    public float ATB = 0f;
    private bool attacker = false;
    public bool Attacker => attacker;

    private Action<CombatUnit> onUnitUpdateUI;
    private UnitContainer unit;
    public UnitContainer Container => unit;
    public void Set(UnitContainer unit, bool attacker)
    {
        this.unit = unit;
        this.attacker = attacker;
        meshFilter.mesh = unit.Data.Mesh;
        ATB = 0f;
        hp = unit.Data.HP;
    }
    public void UpdateATB(float time)
    {
        float initiativePerTurn = unit.Data.Initiative / 10f;
        ATB += initiativePerTurn * time;
    }
    public int TakeDamage(int damage)
    {
        hp -= damage;
        while(hp <= 0)
        {
            Container.DecreaseCount();
            hp += Container.Data.HP;
            Debug.Log(name + " lost one unit");
        }
        OnUnitUpdateUI();
        return Container.Count;
    }
    public void OnUnitUpdateUI()
    {
        if (onUnitUpdateUI != null) onUnitUpdateUI(this);
    }
    public void AddActionOnUnitUpdateUI(Action<CombatUnit> action)
    {
        onUnitUpdateUI += action;
    }
    public bool IsOpponent(CombatUnit unitOther)
    {
        return Container.Player != unitOther.Container.Player;
    }
    public bool IsOpponent(Player player)
    {
        return Container.Player != player;
    }
}
