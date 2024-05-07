using System;
using System.Collections;
using UnityEngine;

public class CombatUnit : MonoBehaviour
{
    private UnitVisual unitVisual = null;
    public UnitVisual Visual => unitVisual;
    private int hp = 0;
    public int HP => hp;
    private float atb = 0f;
    public float ATB => atb;
    private bool attacker = false;
    public bool Attacker => attacker;
    public bool retaliate = false;

    private Action<CombatUnit> onUnitUpdateUI;
    private UnitContainer unit;
    public UnitContainer Container => unit;
    public void Set(UnitContainer unit, bool attacker)
    {
        this.unit = unit;
        this.attacker = attacker;
        unitVisual = Instantiate(unit.Data.VisualPrefab, transform);
        atb = 0f;
        hp = unit.Data.HP;
    }
    public void UpdateATB(float time)
    {
        float initiativePerTurn = unit.Data.Initiative / 10f;
        atb += initiativePerTurn * time;
    }
    public void ResetATB()
    {
        atb -= 1f;
        if(atb < 0f) atb = 0f;
    }
    public void ForceSetATB(float value)
    {
        atb = value;
    }
    public int TakeDamage(int damage)
    {
        hp -= damage;
        while(hp <= 0)
        {
            Container.DecreaseCount();
            Debug.Log(name + " lost one unit");
            if(Container.Count <= 0)
            {
                Debug.Log("lost all stack");
                break;
            }
            hp += Container.Data.HP;
        }
        OnUnitUpdateUI();
        if(hp <= 0)
        {
            Visual.Animator.SetTrigger("Dead");
        }
        else
        {
            Visual.Animator.SetTrigger("TakeDamage");
        }
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
