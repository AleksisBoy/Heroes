using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBarCombatPreparation : UnitBarDynamic
{
    [Header("Combat Preparation")]
    [SerializeField] private CanvasUnitUtility canvasUnitUtility = null;
    [SerializeField] private CombatUnit unitPrefab = null;

    private CombatTile currentTile = null;
    private List<CombatTile> tilesUsed = new List<CombatTile>();
    private CombatUnit currentUnit = null;
    private CombatMap map;
    private bool attacker;
    public void Setup(HeroMount mount, List<CombatTile> tilesUsed, CombatMap map, bool attacker)
    {
        this.tilesUsed = tilesUsed;
        this.map = map;
        this.attacker = attacker;
        base.Setup(mount);
    }
    protected override void UnitIconBeginDrag()
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
    protected override void UnitIconDrag()
    {
        currentTile?.UpdateState(CombatTile.State.Active);
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
                currentTile.UpdateState(CombatTile.State.Selected);

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
    protected override void UnitIconEndDrag(IconContainerUI container)
    {
        if (currentTile == null)
        {
            canvasUnitUtility.DeleteUnitCount(currentUnit);
            Destroy(currentUnit.gameObject);
            return;
        }

        if (currentTile.Unit != null)
        {
            map.RemoveUnitPrepare(currentTile.Unit.Container, attacker);
        }
        if (attacker) map.AddAttackerUnitOnMapPrepare(currentUnit, currentTile);
        else map.AddDefenderUnitOnMapPrepare(currentUnit, currentTile);

        currentTile.UpdateState(CombatTile.State.None);

        currentUnit = null;
        currentTile = null;

        // Update unit bar with showing if unit is placed on map by graying out the frame
    }
}
