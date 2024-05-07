using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CanvasUnitUtility : MonoBehaviour
{
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
    }

    private void Update()
    {
        
    }
    public (CombatTile,  Vector2) HighlightSelection(List<CombatTile> activeTiles, CombatTile actingUnitTile)
    {
        var selection = SelectTileOnPointer();
        CombatTile selectedTile = selection.Item1;
        Vector2 direction = selection.Item2;
        if(selectedTile != currentTile)
        {
            //currentTile?.UpdateState(lastStateTile);
            currentTile?.RemoveState(CombatTile.State.Selected);
            if (activeTiles.Contains(selectedTile))
            {
                if(selectedTile.Unit) selectedTile = map.GetAdjacentTileInDirectionWithin(map.GetUnitTile(selectedTile.Unit.Container), activeTiles, direction, actingUnitTile);
                //selectedTile.UpdateState(CombatTile.State.Selected);
                selectedTile.AddState(CombatTile.State.Selected);
                currentTile = selectedTile;
            }
            else
            {
                currentTile = null;
            }
        }
        return selection;
    }
    public void PreviewMovementTiles(CombatTile tile, List<CombatTile> exceptions)
    {
        ResetLastPreviewTiles();
        resetLastPreviewTiles = false;
        List<CombatTile> previewTiles = map.GetTilesAroundTile(tile, tile.Unit.Container.Data.Speed);
        foreach(CombatTile exception in exceptions)
        {
            if (previewTiles.Contains(exception)) previewTiles.Remove(exception);
        }
        foreach(CombatTile previewTile in previewTiles)
        {
            //previewTile.UpdateState(CombatTile.State.Highlight);
            previewTile.AddState(CombatTile.State.Preview);
        }
        lastPreviewTiles = previewTiles;
    }
    public void ResetLastPreviewTiles()
    {
        if (resetLastPreviewTiles) return;
        resetLastPreviewTiles = true;

        foreach(CombatTile previewTile in lastPreviewTiles)
        {
            //previewTile.UpdateToPreviousState();
            previewTile.RemoveState(CombatTile.State.Preview);
        }
    }
    public void ResetSelection()
    {
        currentTile = null;
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
