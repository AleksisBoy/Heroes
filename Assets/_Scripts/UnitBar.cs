using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBar : MonoBehaviour
{
    [SerializeField] private CombatUnit unitPrefab = null;
    [SerializeField] private CanvasUnitUtility canvasUnitUtility = null;
    [SerializeField] private IconUI iconPrefab = null;
    [SerializeField] private IconContainerUI heroIcon = null;
    [SerializeField] private IconContainerUI[] unitIcons = null;

    private CombatTile currentTile = null;
    private List<CombatTile> tilesUsed = new List<CombatTile>();
    private CombatUnit currentUnit = null;
    private HeroMount mount;
    private CombatMap map;
    private bool attacker;
    public void Setup(HeroMount mount, List<CombatTile> tilesUsed, CombatMap map, bool attacker)
    {
        this.tilesUsed = tilesUsed;
        this.mount = mount;
        this.map = map;
        this.attacker = attacker;

        IconUI icon = Instantiate(iconPrefab);
        icon.Set(typeof(HeroMount).ToString(), mount.Hero.name, -1, mount.Hero.Sprite, 1, heroIcon, false);

        for(int i = 0; i < mount.Units.Length; i++)
        {
            if (mount.Units[i] == null) continue;

            IconUI iconUnit = Instantiate(iconPrefab);
            iconUnit.AddOnDrag(UnitIconDrag);
            iconUnit.AddOnEndDrag(UnitIconEndDrag);
            iconUnit.AddOnBeginDrag(UnitIconBeginDrag);
            iconUnit.Set(typeof(Unit).ToString(), mount.Units[i].Data.name, i, mount.Units[i].Data.Sprite, mount.Units[i].Count, unitIcons[i], true);
        }
    }
    private void UnitIconBeginDrag()
    {
        currentUnit = Instantiate(unitPrefab);
        UnitContainer unit = mount.GetUnitContainerFromIconData(IconUI.Current.Data);
        if (unit == null)
        {
            Debug.LogError("NO CORRESPONDING UNIT FOUND IN HERO MOUNT");
            Destroy(currentUnit.gameObject);
            return;
        }
        currentUnit.Set(unit, attacker);

        if (map.IsUnitOnMapPrepare(currentUnit, attacker))
        {
            map.RemoveUnitPrepare(currentUnit.Container, attacker);
        }
        currentUnit.gameObject.SetActive(false);
    }
    private void UnitIconDrag()
    {
        currentTile?.Actived();
        if (RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, Input.mousePosition))
        {
            // if mouse over unit bar
            IconUI.Current.EnableImage(true);
            currentUnit.gameObject.SetActive(false);

            currentTile = null;
        }
        else
        {
            // if mouse over map
            IconUI.Current.EnableImage(false);
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);
            CombatTile selectedTile = map.GetTileInPosition(hitInfo.point);
            if (tilesUsed.Contains(selectedTile))
            {
                currentTile = selectedTile;
                currentTile.Selected();

                currentTile.ShowUnitPlaceHolder(currentUnit);
                canvasUnitUtility.UpdateUnitCount(currentUnit, map);
            }
            else
            {
                currentUnit.gameObject.SetActive(false);
                canvasUnitUtility.EnableUnitCount(currentUnit, false);
                currentTile = null;
            }
        }
    }
    private void UnitIconEndDrag(IconContainerUI container)
    {
        if (currentTile == null)
        {
            canvasUnitUtility.DeleteUnitCount(currentUnit);
            Destroy(currentUnit.gameObject);
            return;
        }

        if(currentTile.Unit != null)
        {
            map.RemoveUnitPrepare(currentTile.Unit.Container, attacker);
        }
        if (attacker) map.AddAttackerUnitOnMapPrepare(currentUnit, currentTile);
        else map.AddDefenderUnitOnMapPrepare(currentUnit, currentTile);

        currentTile.Inactived();

        currentUnit = null;
        currentTile = null;

        // Update unit bar with showing if unit is placed on map by graying out the frame
    }
}
