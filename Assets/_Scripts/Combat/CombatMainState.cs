using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatMainState : MonoBehaviour
{
    [SerializeField] private CanvasUnitUtility canvasUnitUtility = null;

    protected CombatMap map = null;
    protected CombatTile selectedTile = null;
    private Vector2 direction;
    protected List<CombatTile> activeTiles = null;
    protected CombatTile actingUnitTile = null;
    public CombatTile ActingUnitTile => actingUnitTile;
    protected bool isActive = false;
    protected virtual void Update()
    {
        if (isActive)
        {
            var selection = canvasUnitUtility.HighlightSelection(activeTiles, actingUnitTile);
            if (!selection.Item1) return;

            if (selection.Item1.Unit && selection.Item1 != actingUnitTile) canvasUnitUtility.PreviewMovementTiles(selection.Item1, new List<CombatTile>());
            else canvasUnitUtility.ResetLastPreviewTiles();

            if (Input.GetMouseButtonDown(0))
            {
                direction = selection.Item2;
                if (activeTiles.Contains(selection.Item1)) selectedTile = selection.Item1;
            }
        }

        // can check unit popup either if isActive or no
        if (Input.GetMouseButtonDown(1))
        {
            canvasUnitUtility.SetUnitPopup();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            canvasUnitUtility.DisableUnitPopup();
        }
    }
    public virtual void StartCombat(CombatMap map, Player player)
    {
        this.map = map;
        selectedTile = null;
        gameObject.SetActive(true);
        List<CombatUnit> allUnits = map.GetAllUnits();
        canvasUnitUtility.SetAllUnits(allUnits);
        foreach(CombatUnit unit in allUnits)
        {
            canvasUnitUtility.UpdateUnitCount(unit, map);
        }
    }
    public void SetActive(List<CombatTile> activeTiles, bool state, CombatTile actingUnitTile)
    {
        this.activeTiles = activeTiles;
        this.actingUnitTile = actingUnitTile;
        isActive = state;
        if (!state)
        {
            selectedTile = null;
            canvasUnitUtility?.ResetSelection();
        }
    }
    public void DeactivateTiles()
    {
        map.DeactivateTiles();
    }
    public CombatTile GetSelectedTile()
    {
        return selectedTile;
    }
    public Vector2 GetSelectedTileDirection()
    {
        return direction;
    }

}
