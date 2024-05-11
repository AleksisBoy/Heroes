using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class CanvasUnitUtility : MonoBehaviour
{
    [SerializeField] private PreviewDamageUI previewDamageUI = null;
    [SerializeField] private Transform unitCountParent = null;
    [SerializeField] private UnitCountUI unitCountUIPrefab = null;
    [SerializeField] private PopupCombatUnit popupUnit = null;
    [SerializeField] private Color allyColor = Color.green;
    [SerializeField] private Color enemyColor = Color.red;

    private Player player = null;
    private List<CombatUnit> allUnits = new List<CombatUnit>();

    private CombatMap map;
    private List<UnitCountUI> unitCounts = new List<UnitCountUI>();

    private CombatTile currentTile = null;
    private CombatTile lastPreviewTile = null;
    private List<CombatTile> lastPreviewTiles = new List<CombatTile>();
    private bool resetLastPreviewTiles = false;
    public CombatTile Selection => currentTile;
    private void Awake()
    {
        foreach(Transform child in unitCountParent)
        {
            Destroy(child.gameObject);
        }
        popupUnit.gameObject.SetActive(false);
        previewDamageUI.gameObject.SetActive(false);
    }

    public (CombatTile,  Vector2) HighlightSelection(List<CombatTile> activeTiles, CombatTile actingUnitTile, out bool selectedTileIsAdjacent)
    {
        selectedTileIsAdjacent = false;
        var selection = SelectTileOnPointer();

        if (selection.Item1 == null && CombatUnitIconUI.HoverOver)
        {
            selection.Item1 = map.GetUnitTile(CombatUnitIconUI.HoverOver.Unit.Container);
            Vector3 dir3D = (actingUnitTile.transform.position - selection.Item1.transform.position).normalized;
            selection.Item2 = new Vector2(dir3D.x, dir3D.z);
        }

        CombatTile selectedTile = selection.Item1;
        Vector2 direction = selection.Item2;
        if(selectedTile != currentTile)
        {
            currentTile?.RemoveState(CombatTile.State.Selected);
            if (activeTiles.Contains(selectedTile))
            {
                if (selectedTile.Unit && actingUnitTile.Unit.Container.Player != selectedTile.Unit.Container.Player)
                {
                    PreviewDamage(actingUnitTile.Unit, selectedTile.Unit);
                    selectedTile = map.GetAdjacentTileInDirectionWithin(map.GetUnitTile(selectedTile.Unit.Container), activeTiles, direction, actingUnitTile, out float angle);
                    SwitchCursorToAttacking(angle);
                    selectedTileIsAdjacent = true;
                }
                else if(selectedTile.Unit)
                {
                    InternalSettings.SwitchCursorTo(InternalSettings.CursorState.Blocked);
                    ResetPreviewDamage();
                }
                else
                {
                    InternalSettings.SwitchCursorTo(InternalSettings.CursorState.Select);
                    ResetPreviewDamage();
                }

                selectedTile.AddState(CombatTile.State.Selected);
                currentTile = selectedTile;
            }
            else
            {
                currentTile = null;
                ResetPreviewDamage();
                InternalSettings.SwitchCursorTo(InternalSettings.CursorState.Blocked);
            }
        }
        else
        {
            ResetPreviewDamage();
            InternalSettings.SwitchCursorTo(InternalSettings.CursorState.Select);
        }
        return selection;
    }
    private void PreviewDamage(CombatUnit attackingUnit, CombatUnit defendingUnit)
    {
        previewDamageUI.Set(CombatManager.GetDamageRange(attackingUnit, defendingUnit), defendingUnit.retaliate);
    }
    public void ResetPreviewDamage()
    {
        previewDamageUI.gameObject.SetActive(false);
    }
    public void PreviewMovementTiles(CombatTile tile, List<CombatTile> exceptions)
    {
        if (lastPreviewTile == tile) return;
        ResetLastPreviewTiles();
        resetLastPreviewTiles = false;

        List<CombatTile> previewTiles = map.GetTilesAroundTile(tile, tile.Unit.Container.Data.Speed);
        foreach(CombatTile exception in exceptions)
        {
            if (previewTiles.Contains(exception)) previewTiles.Remove(exception);
        }
        foreach(CombatTile previewTile in previewTiles)
        {
            previewTile.AddState(CombatTile.State.Preview);
        }
        lastPreviewTiles = previewTiles;
        lastPreviewTile = tile;
    }
    public void ResetLastPreviewTiles()
    {
        if (resetLastPreviewTiles) return;
        resetLastPreviewTiles = true;
        lastPreviewTile = null;

        foreach (CombatTile previewTile in lastPreviewTiles)
        {
            //previewTile.UpdateToPreviousState();
            previewTile.RemoveState(CombatTile.State.Preview);
        }
    }
    private void SwitchCursorToAttacking(float angle)
    {
        if (angle >= 0.0625f && angle < 0.1875f)
        {
            InternalSettings.SwitchCursorTo(InternalSettings.CursorState.AttackingBottomLeft);
        }
        else if (angle >= 0.1875f && angle < 0.3125f)
        {
            InternalSettings.SwitchCursorTo(InternalSettings.CursorState.AttackingBottom);
        }
        else if (angle >= 0.3125f && angle < 0.4375f)
        {
            InternalSettings.SwitchCursorTo(InternalSettings.CursorState.AttackingBottomRight);
        }
        else if (angle >= 0.4375 && angle < 0.5625f)
        {
            InternalSettings.SwitchCursorTo(InternalSettings.CursorState.AttackingRight);
        }
        else if (angle >= 0.5625f && angle < 0.6875f)
        {
            InternalSettings.SwitchCursorTo(InternalSettings.CursorState.AttackingTopRight);
        }
        else if (angle >= 0.6875f && angle < 0.8125f)
        {
            InternalSettings.SwitchCursorTo(InternalSettings.CursorState.AttackingTop);
        }
        else if (angle >= 0.8125f && angle < 0.9375f)
        {
            InternalSettings.SwitchCursorTo(InternalSettings.CursorState.AttackingTopLeft);
        }
        else
        {
            InternalSettings.SwitchCursorTo(InternalSettings.CursorState.AttackingLeft);
        }
    }
    public void ResetSelection()
    {
        currentTile = null;
        ResetPreviewDamage();
    }
    public void SetUnitPopup() // on mouse down
    {
        CombatTile selectedTile = SelectTileOnPointer().Item1;
        if (!selectedTile) return;

        CombatUnit unit = selectedTile.Unit;
        if (!unit) return;

        popupUnit.Set(unit);
    }
    public void DisableUnitPopup()
    {
        popupUnit.gameObject.SetActive(false);
    }
    public (CombatTile, Vector2) SelectTileOnPointer()
    {
        Heroes_StandaloneInputModule sim = EventSystem.current.currentInputModule as Heroes_StandaloneInputModule; 
        if (sim.GetCurrent()
            && sim.GetCurrent().layer == LayerMask.NameToLayer("UI")) return (null, Vector2.zero);

        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);
        if (hitInfo.transform == null) return (null, Vector2.zero);

        CombatTile tile = map.GetTileInPosition(hitInfo.point);
        if(!tile) return (null, Vector2.zero);

        Vector3 direction = hitInfo.point - tile.transform.position;
        direction.Normalize();
        return (tile, new Vector2(direction.x, direction.z));
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
            unitCountUI = Instantiate(unitCountUIPrefab, unitCountParent);
            unitCountUI.SetUI(unit, map.TileScale, unit.Container.Player == player ? allyColor : enemyColor);
            unitCounts.Add(unitCountUI);
            unit.AddActionOnUnitUpdateUI(UpdateUnitCount);
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
    public void SetAllUnits(List<CombatUnit> allUnits)
    {
        this.allUnits = allUnits;
    }
    public void SetPlayer(Player player)
    {
        this.player = player;
    }
}
