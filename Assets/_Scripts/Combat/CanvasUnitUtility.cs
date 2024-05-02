using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CanvasUnitUtility : MonoBehaviour
{
    [SerializeField] private Player player = null;
    [SerializeField] private UnitCountUI unitCountUIPrefab = null;
    [SerializeField] private Color allyColor = Color.green;
    [SerializeField] private Color enemyColor = Color.red;

    private CombatMap map;
    private List<UnitCountUI> unitCounts = new List<UnitCountUI>();
    private void Awake()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void UpdateUnitCount(CombatUnit unit, CombatMap map)
    {
        UnitCountUI unitCountUI = unitCounts.FirstOrDefault(x => x.Unit == unit);
        if (unitCountUI)
        {
            unitCountUI.UpdateCount();
            unitCountUI.UpdatePosition();
            unitCountUI.gameObject.SetActive(true);
        }
        else
        {
            unitCountUI = Instantiate(unitCountUIPrefab, transform);
            unitCountUI.SetUI(unit, map.TileScale, unit.Container.Player == player ? allyColor : enemyColor);
            unitCounts.Add(unitCountUI);
            unit.AddActionOnUnitMoved(UpdateUnitCount);
            if (this.map == null)
            {
                this.map = map;
                this.map.AddActionOnUnitRemove(DeleteUnitCount);
            }
        }
        if (!gameObject.activeSelf) gameObject.SetActive(true);
    }
    private void UpdateUnitCount(CombatUnit unit)
    {
        UpdateUnitCount(unit, null);
    }
    public void DeleteUnitCount(CombatUnit unit)
    {
        UnitCountUI unitCountUI = unitCounts.FirstOrDefault(x => x.Unit == unit);
        if (!unitCountUI) return;

        unitCounts.Remove(unitCountUI);
        Destroy(unitCountUI.gameObject);
    }
    public void EnableUnitCount(CombatUnit unit, bool state)
    {
        UnitCountUI unitCountUI = unitCounts.FirstOrDefault(x => x.Unit == unit);
        if (!unitCountUI) return;

        unitCountUI.gameObject.SetActive(state);
    }
}
